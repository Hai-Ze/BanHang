using BanHang.Models;
using BanHang.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static BanHang.Models.Order;

namespace BanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            UserManager<ApplicationUser> userManager)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userManager = userManager;
        }

        // GET: Admin/Order
        public async Task<IActionResult> Index(string status = "", string search = "", DateTime? fromDate = null, DateTime? toDate = null)
        {
            IEnumerable<Order> orders;

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var orderStatus))
            {
                orders = await _orderRepository.GetByStatusAsync(orderStatus);
            }
            // Lọc theo khoảng thời gian
            else if (fromDate.HasValue && toDate.HasValue)
            {
                orders = await _orderRepository.GetByDateRangeAsync(fromDate.Value, toDate.Value.AddDays(1));
            }
            else
            {
                orders = await _orderRepository.GetAllAsync();
            }

            // Tìm kiếm theo mã đơn hàng hoặc tên khách hàng
            if (!string.IsNullOrEmpty(search))
            {
                orders = orders.Where(o =>
                    o.OrderNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    o.CustomerName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    o.CustomerEmail.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Truyền dữ liệu cho filter
            ViewBag.StatusList = new SelectList(Enum.GetValues(typeof(OrderStatus))
                .Cast<OrderStatus>()
                .Select(s => new { Value = s.ToString(), Text = GetStatusDisplayName(s) }),
                "Value", "Text", status);

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = search;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(orders);
        }

        // GET: Admin/Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // GET: Admin/Order/Create
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Create()
        {
            var products = await _productRepository.GetAllAsync();
            ViewBag.Products = new SelectList(products, "Id", "Name");

            var customers = await _userManager.GetUsersInRoleAsync(SD.Role_Customer);
            ViewBag.Customers = new SelectList(customers, "Id", "FullName");

            var order = new Order
            {
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                OrderDetails = new List<OrderDetail>()
            };

            return View(order);
        }

        // POST: Admin/Order/Create
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Create(Order order, List<int> productIds, List<int> quantities)
        {
            if (productIds == null || !productIds.Any())
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một sản phẩm.");
            }

            if (ModelState.IsValid)
            {
                order.OrderDate = DateTime.Now;
                order.OrderNumber = await _orderRepository.GenerateOrderNumberAsync();
                order.OrderDetails = new List<OrderDetail>();

                decimal totalAmount = 0;

                for (int i = 0; i < productIds.Count; i++)
                {
                    if (quantities[i] > 0)
                    {
                        var product = await _productRepository.GetByIdAsync(productIds[i]);
                        if (product != null)
                        {
                            var orderDetail = new OrderDetail
                            {
                                ProductId = product.Id,
                                ProductName = product.Name,
                                UnitPrice = product.Price,
                                Quantity = quantities[i],
                                TotalPrice = product.Price * quantities[i],
                                ProductImageUrl = product.ImageUrl
                            };

                            order.OrderDetails.Add(orderDetail);
                            totalAmount += orderDetail.TotalPrice;
                        }
                    }
                }

                order.TotalAmount = totalAmount;
                order.FinalAmount = totalAmount + order.ShippingFee - order.DiscountAmount;

                await _orderRepository.AddAsync(order);
                return RedirectToAction(nameof(Index));
            }

            // Reload dropdown data if validation fails
            var products = await _productRepository.GetAllAsync();
            ViewBag.Products = new SelectList(products, "Id", "Name");

            var customers = await _userManager.GetUsersInRoleAsync(SD.Role_Customer);
            ViewBag.Customers = new SelectList(customers, "Id", "FullName");

            return View(order);
        }

        // GET: Admin/Order/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            ViewBag.StatusList = new SelectList(Enum.GetValues(typeof(OrderStatus))
                .Cast<OrderStatus>()
                .Select(s => new { Value = s, Text = GetStatusDisplayName(s) }),
                "Value", "Text", order.Status);

            return View(order);
        }

        // POST: Admin/Order/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingOrder = await _orderRepository.GetByIdAsync(id);
                if (existingOrder == null)
                {
                    return NotFound();
                }

                // Cập nhật các trường được phép thay đổi
                existingOrder.Status = order.Status;
                existingOrder.CustomerName = order.CustomerName;
                existingOrder.CustomerEmail = order.CustomerEmail;
                existingOrder.CustomerPhone = order.CustomerPhone;
                existingOrder.ShippingAddress = order.ShippingAddress;
                existingOrder.Notes = order.Notes;
                existingOrder.ShippingFee = order.ShippingFee;
                existingOrder.DiscountAmount = order.DiscountAmount;
                existingOrder.FinalAmount = existingOrder.TotalAmount + order.ShippingFee - order.DiscountAmount;

                // Cập nhật ngày dựa trên trạng thái
                switch (order.Status)
                {
                    case OrderStatus.Shipping:
                        if (existingOrder.ShippedDate == null)
                            existingOrder.ShippedDate = DateTime.Now;
                        break;
                    case OrderStatus.Delivered:
                        if (existingOrder.DeliveredDate == null)
                            existingOrder.DeliveredDate = DateTime.Now;
                        break;
                    case OrderStatus.Cancelled:
                        if (existingOrder.CancelledDate == null)
                            existingOrder.CancelledDate = DateTime.Now;
                        existingOrder.CancelReason = order.CancelReason;
                        break;
                }

                await _orderRepository.UpdateAsync(existingOrder);
                return RedirectToAction(nameof(Details), new { id = existingOrder.Id });
            }

            ViewBag.StatusList = new SelectList(Enum.GetValues(typeof(OrderStatus))
                .Cast<OrderStatus>()
                .Select(s => new { Value = s, Text = GetStatusDisplayName(s) }),
                "Value", "Text", order.Status);

            return View(order);
        }

        // POST: Admin/Order/UpdateStatus/5
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status, string cancelReason = "")
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            order.Status = status;

            switch (status)
            {
                case OrderStatus.Shipping:
                    order.ShippedDate = DateTime.Now;
                    break;
                case OrderStatus.Delivered:
                    order.DeliveredDate = DateTime.Now;
                    break;
                case OrderStatus.Cancelled:
                    order.CancelledDate = DateTime.Now;
                    order.CancelReason = cancelReason;
                    break;
            }

            await _orderRepository.UpdateAsync(order);

            return Json(new
            {
                success = true,
                message = $"Đã cập nhật trạng thái đơn hàng thành '{GetStatusDisplayName(status)}'"
            });
        }

        // GET: Admin/Order/Delete/5
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Admin/Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _orderRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Thống kê đơn hàng
        public async Task<IActionResult> Statistics()
        {
            var totalRevenue = await _orderRepository.GetTotalRevenueAsync();
            var orderCount = await _orderRepository.GetOrderCountAsync();
            var recentOrders = await _orderRepository.GetRecentOrdersAsync(5);

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.OrderCount = orderCount;
            ViewBag.RecentOrders = recentOrders;

            return View();
        }

        private string GetStatusDisplayName(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Chờ xác nhận",
                OrderStatus.Confirmed => "Đã xác nhận",
                OrderStatus.Processing => "Đang chuẩn bị",
                OrderStatus.Shipping => "Đang giao hàng",
                OrderStatus.Delivered => "Đã giao hàng",
                OrderStatus.Cancelled => "Đã hủy",
                OrderStatus.Returned => "Hoàn trả",
                _ => status.ToString()
            };
        }
    }
}