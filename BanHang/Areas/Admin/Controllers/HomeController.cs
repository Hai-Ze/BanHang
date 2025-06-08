using BanHang.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace BanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]  
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context; // Khai báo biến trường

        // Constructor để inject DbContext
        public HomeController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // Kiểm tra null
        }

        public IActionResult Dashboard()
        {
            return View("Index");
        }
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            var userCount = await _context.Users.CountAsync(); // Số người dùng từ AspNetUsers
            var categoryCount = await _context.Categories.CountAsync(); 

            ViewBag.UserCount = userCount;
            ViewBag.CategoryCount = categoryCount;
            return View(products);
        }
    }
}