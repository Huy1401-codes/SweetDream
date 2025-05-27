using SweetDream.Models;
using SweetDream.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SweetDream.Controllers
{
    [Authorize] // Yêu cầu đăng nhập
    public class FeedbacksController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<Account> _userManager;

        public FeedbacksController(DataContext context, UserManager<Account> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Hiển thị form đánh giá
        public async Task<IActionResult> Create(int detailId)
        {
            var orderDetail = await _context.OrderDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(od => od.DetailId == detailId);

            if (orderDetail == null || orderDetail.IsReviewed || orderDetail.Status != "Success")
            {
                TempData["Error"] = "Chỉ có thể đánh giá khi sản phẩm đã được giao thành công.";
                return RedirectToAction("OrderDetails", "Profiles");
            }

            var feedback = new Feedback { DetailId = detailId, ProductId = orderDetail.ProductId };
            return View(feedback);
        }


        [HttpPost]
        public async Task<IActionResult> Create(Feedback feedback)
        {
            if (!ModelState.IsValid)
            {
                return View(feedback);
            }

            var orderDetail = await _context.OrderDetails
                .FirstOrDefaultAsync(od => od.DetailId == feedback.DetailId);

            if (orderDetail == null || orderDetail.IsReviewed || orderDetail.Status != "Success")
            {
                TempData["Error"] = "Không thể đánh giá sản phẩm này. Hãy chắc chắn rằng sản phẩm đã được giao thành công.";
                return RedirectToAction("OrderDetails", "Profiles");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Tài khoản không hợp lệ.";
                return RedirectToAction("OrderDetails", "Profiles");
            }

            feedback.AccountId = user.Id;
            feedback.CreatedAt = DateTime.UtcNow;

            _context.Feedbacks.Add(feedback);
            orderDetail.IsReviewed = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
            return RedirectToAction("OrderDetails", "Profiles", new { id = orderDetail.OId });
        }


        // Hiển thị đánh giá của một đơn hàng
        public async Task<IActionResult> Feedback(int detailId)
        {
            var feedback = await _context.Feedbacks
                .Include(f => f.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.DetailId == detailId);

            if (feedback == null)
            {
                TempData["Error"] = "Không tìm thấy đánh giá!";
                return RedirectToAction("OrderDetails", "Profiles");
            }

            ViewData["OrderId"] = await _context.OrderDetails
                .Where(od => od.DetailId == detailId)
                .Select(od => od.OId)
                .FirstOrDefaultAsync();

            return View(feedback);
        }
    }
}
