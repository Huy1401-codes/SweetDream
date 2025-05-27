using SweetDream.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SweetDream.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class DashboardController : Controller
    {
        private readonly DataContext _context;

        public DashboardController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalRevenue = await _context.OrderDetails
                .Include(o => o.Product)
                .SumAsync(o => o.Quantity * o.Product.Price);

            var totalOrders = await _context.Orders.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var totalCustomers = await _context.Users.CountAsync();

            var latestOrders = await _context.Orders
                .Include(o => o.Account)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            var bestSellingProducts = await _context.OrderDetails
                .Include(od => od.Product)
                .GroupBy(od => new { od.ProductId, od.Product.ProductName, od.Product.Image })
                .Select(g => new
                {
                    g.Key.ProductName,
                    g.Key.Image,
                    TotalSold = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(g => g.TotalSold)
                .Take(5)
                .ToListAsync();

            // Doanh thu theo từng tháng
            var revenueByMonth = await _context.OrderDetails
                .Include(od => od.Order)
                .Where(od => od.Order.OrderDate.Year == DateTime.Now.Year) 
                .GroupBy(od => od.Order.OrderDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(od => od.Quantity * od.Product.Price)
                })
                .OrderBy(g => g.Month)
                .ToListAsync();

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.LatestOrders = latestOrders;
            ViewBag.BestSellingProducts = bestSellingProducts;

            // Chuyển danh sách revenueByMonth thành JSON để sử dụng trong JavaScript
            ViewBag.RevenueData = Newtonsoft.Json.JsonConvert.SerializeObject(revenueByMonth);

            return View();
        }

    }
}
