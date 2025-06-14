using BanHang.Models;
using BanHang.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderRepository _orderRepository; // ✅ THÊM ORDER REPOSITORY

        public HomeController(ApplicationDbContext context, IOrderRepository orderRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository)); // ✅ INJECT ORDER REPO
        }

        public IActionResult Dashboard()
        {
            return View("Index");
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            var userCount = await _context.Users.CountAsync();
            var categoryCount = await _context.Categories.CountAsync();

            // ✅ THÊM THỐNG KÊ ĐỚN HÀNG
            var orderCount = await _orderRepository.GetOrderCountAsync();
            var totalRevenue = await _orderRepository.GetTotalRevenueAsync();
            var recentOrders = await _orderRepository.GetRecentOrdersAsync(5);

            ViewBag.UserCount = userCount;
            ViewBag.CategoryCount = categoryCount;
            ViewBag.OrderCount = orderCount; // ✅ THÊM
            ViewBag.TotalRevenue = totalRevenue; // ✅ THÊM
            ViewBag.RecentOrders = recentOrders; // ✅ THÊM

            return View(products);
        }
    }
}