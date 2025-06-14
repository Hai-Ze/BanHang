using BanHang.Extensions;
using BanHang.Models;
using BanHang.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BanHang.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartController(
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            UserManager<ApplicationUser> userManager)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _userManager = userManager;
        }

        // GET: ShoppingCart
        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            return View(cart);
        }

        // POST: Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (quantity <= 0)
            {
                TempData["Error"] = "Số lượng phải lớn hơn 0";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var product = await GetProductFromDatabase(productId);
                if (product == null)
                {
                    TempData["Error"] = "Không tìm thấy sản phẩm";
                    return RedirectToAction("Index", "Home");
                }

                var cartItem = new CartItem
                {
                    ProductId = productId,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl,
                    CategoryName = product.Category?.Name
                };

                var cart = GetCartFromSession();
                cart.AddItem(cartItem);
                SaveCartToSession(cart);

                TempData["Success"] = $"Đã thêm {product.Name} vào giỏ hàng";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "Có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Cập nhật số lượng sản phẩm
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCartFromSession();

            if (quantity <= 0)
            {
                cart.RemoveItem(productId);
                TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng";
            }
            else
            {
                cart.UpdateQuantity(productId, quantity);
                TempData["Success"] = "Đã cập nhật số lượng sản phẩm";
            }

            SaveCartToSession(cart);
            return RedirectToAction("Index");
        }

        // POST: Xóa sản phẩm khỏi giỏ hàng
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCartFromSession();
            cart.RemoveItem(productId);
            SaveCartToSession(cart);

            TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng";
            return RedirectToAction("Index");
        }

        // POST: Xóa tất cả sản phẩm trong giỏ hàng
        [HttpPost]
        public IActionResult ClearCart()
        {
            var cart = new ShoppingCart();
            SaveCartToSession(cart);

            TempData["Success"] = "Đã xóa tất cả sản phẩm trong giỏ hàng";
            return RedirectToAction("Index");
        }

        // ✅ GET: Trang Checkout
        [Authorize] // Yêu cầu đăng nhập
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCartFromSession();

            if (cart.IsEmpty)
            {
                TempData["Error"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi đặt hàng.";
                return RedirectToAction("Index");
            }

            // Lấy thông tin user hiện tại
            var user = await _userManager.GetUserAsync(User);

            var checkoutViewModel = new CheckoutViewModel
            {
                Cart = cart,
                CustomerName = user?.FullName ?? "",
                CustomerEmail = user?.Email ?? "",
                CustomerPhone = user?.PhoneNumber ?? "",
                ShippingAddress = user?.Address ?? "",
                Notes = "",
                ShippingFee = CalculateShippingFee(cart),
                DiscountAmount = 0
            };

            return View(checkoutViewModel);
        }

        // ✅ POST: Xử lý Checkout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cart = GetCartFromSession();

            if (cart.IsEmpty)
            {
                TempData["Error"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi đặt hàng.";
                return RedirectToAction("Index");
            }

            // Xác thực dữ liệu
            ModelState.Remove("Cart"); // Loại bỏ validation cho Cart vì không cần validate

            if (!ModelState.IsValid)
            {
                model.Cart = cart;
                model.ShippingFee = CalculateShippingFee(cart);
                return View(model);
            }

            try
            {
                // Lấy thông tin user
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "Không thể xác định thông tin người dùng";
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                // Tạo đơn hàng mới
                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    CustomerId = user.Id,
                    CustomerName = model.CustomerName,
                    CustomerEmail = model.CustomerEmail,
                    CustomerPhone = model.CustomerPhone,
                    ShippingAddress = model.ShippingAddress,
                    Notes = model.Notes ?? "",
                    Status = OrderStatus.Pending,
                    TotalAmount = cart.GetTotalPrice(),
                    ShippingFee = model.ShippingFee,
                    DiscountAmount = model.DiscountAmount,
                    FinalAmount = cart.GetTotalPrice() + model.ShippingFee - model.DiscountAmount,
                    OrderDetails = new List<OrderDetail>()
                };

                // Thêm chi tiết đơn hàng
                foreach (var cartItem in cart.Items)
                {
                    var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                    if (product != null)
                    {
                        var orderDetail = new OrderDetail
                        {
                            ProductId = cartItem.ProductId,
                            ProductName = cartItem.Name,
                            UnitPrice = cartItem.Price,
                            Quantity = cartItem.Quantity,
                            TotalPrice = cartItem.TotalPrice,
                            ProductImageUrl = cartItem.ImageUrl
                        };
                        order.OrderDetails.Add(orderDetail);
                    }
                }

                // Lưu đơn hàng vào database
                await _orderRepository.AddAsync(order);

                // Xóa giỏ hàng sau khi đặt hàng thành công
                ClearCartSession();

                // Chuyển đến trang xác nhận
                TempData["OrderSuccess"] = $"Đặt hàng thành công! Mã đơn hàng: {order.OrderNumber}";
                return RedirectToAction("OrderConfirmation", new { orderNumber = order.OrderNumber });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại.";
                model.Cart = cart;
                model.ShippingFee = CalculateShippingFee(cart);
                return View(model);
            }
        }

        // ✅ GET: Trang xác nhận đơn hàng
        public async Task<IActionResult> OrderConfirmation(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber))
            {
                return RedirectToAction("Index", "Home");
            }

            var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("Index", "Home");
            }

            // Chỉ cho phép user xem đơn hàng của mình (trừ Admin)
            var user = await _userManager.GetUserAsync(User);
            if (user == null || (order.CustomerId != user.Id && !User.IsInRole("Admin")))
            {
                TempData["Error"] = "Bạn không có quyền xem đơn hàng này";
                return RedirectToAction("Index", "Home");
            }

            return View(order);
        }

        // ✅ GET: Lịch sử đơn hàng của user
        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var orders = await _orderRepository.GetByCustomerIdAsync(user.Id);
            return View(orders);
        }

        // GET: Lấy số lượng sản phẩm trong giỏ hàng (cho header)
        public IActionResult GetCartCount()
        {
            var cart = GetCartFromSession();
            return Json(cart.GetTotalQuantity());
        }

        // GET: Lấy thông tin giỏ hàng (AJAX)
        public IActionResult GetCartInfo()
        {
            var cart = GetCartFromSession();
            return Json(new
            {
                itemCount = cart.ItemCount,
                totalQuantity = cart.GetTotalQuantity(),
                totalPrice = cart.GetTotalPrice(),
                isEmpty = cart.IsEmpty
            });
        }

        #region Private Methods

        /// <summary>
        /// Lấy giỏ hàng từ session
        /// </summary>
        private ShoppingCart GetCartFromSession()
        {
            return HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
        }

        /// <summary>
        /// Lưu giỏ hàng vào session
        /// </summary>
        private void SaveCartToSession(ShoppingCart cart)
        {
            HttpContext.Session.SetObjectAsJson("Cart", cart);
        }

        /// <summary>
        /// Xóa giỏ hàng khỏi session
        /// </summary>
        private void ClearCartSession()
        {
            HttpContext.Session.Remove("Cart");
        }

        /// <summary>
        /// Lấy thông tin sản phẩm từ database
        /// </summary>
        private async Task<Product> GetProductFromDatabase(int productId)
        {
            return await _productRepository.GetByIdAsync(productId);
        }

        /// <summary>
        /// Tính phí vận chuyển
        /// </summary>
        private decimal CalculateShippingFee(ShoppingCart cart)
        {
            // Logic tính phí vận chuyển
            var totalAmount = cart.GetTotalPrice();

            if (totalAmount >= 500000) // Miễn phí vận chuyển cho đơn hàng trên 500k
                return 0;
            else if (totalAmount >= 200000) // Giảm 50% phí vận chuyển cho đơn hàng trên 200k
                return 15000;
            else
                return 30000; // Phí vận chuyển tiêu chuẩn
        }

        #endregion
    }
}