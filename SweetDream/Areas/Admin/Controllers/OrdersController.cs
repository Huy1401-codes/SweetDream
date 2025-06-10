using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SweetDream.Repositories;
using X.PagedList;

namespace SweetDream.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class OrdersController : Controller
    {
        private readonly DataContext _context;

        public OrdersController(DataContext context)
        {
            _context = context;
        }



        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string searchName, int page = 1)
        {
        int pageSize = 5; // Số đơn hàng trên mỗi trang

        var ordersQuery = _context.Orders
            .Where(o => !o.IsCart)
            .Include(o => o.Account)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .AsQueryable();

        // Lọc theo khoảng ngày
        if (startDate.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.OrderDetails.Any(od => od.OrderDate >= startDate.Value));
        }
        if (endDate.HasValue)
        {
            DateTime endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
            ordersQuery = ordersQuery.Where(o => o.OrderDetails.Any(od => od.OrderDate <= endOfDay));
        }

        // Lọc theo tên khách hàng
        if (!string.IsNullOrEmpty(searchName))
        {
            ordersQuery = ordersQuery.Where(o => o.Account != null &&
                                                 (o.Account.FirstName + " " + o.Account.LastName)
                                                 .Contains(searchName.Trim()));
        }

        var orders = await ordersQuery.OrderByDescending(o => o.OId).ToPagedListAsync(page, pageSize);

        return View(orders);
    }



    public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Account)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OId == id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int detailId, string status)
        {
            var orderDetail = await _context.OrderDetails.FindAsync(detailId);
            if (orderDetail == null)
            {
                return NotFound();
            }

            if (orderDetail.Status == "Success" || orderDetail.Status == "Disable")
            {
                return BadRequest("Không thể cập nhật trạng thái của sản phẩm đã hoàn thành hoặc bị vô hiệu hóa.");
            }

            var validStatuses = new List<string> { "Processing", "Delivering", "Success", "Disable" };
            if (!validStatuses.Contains(status))
            {
                return BadRequest("Trạng thái không hợp lệ.");
            }

            orderDetail.Status = status;

            if (status == "Success")
            {
                orderDetail.ReceiveDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = orderDetail.OId });
        }


    }
}
