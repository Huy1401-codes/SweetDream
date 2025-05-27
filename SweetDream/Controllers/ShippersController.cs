using SweetDream.Models;
using SweetDream.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace SweetDream.Controllers
{
    [Authorize(Roles = "Shipper")]
    public class ShippersController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<Account> _userManager;

        public ShippersController(DataContext context, UserManager<Account> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Hiển thị danh sách đơn hàng chưa được nhận
        public async Task<IActionResult> UnassignedOrders()
        {
            var orders = await _context.Orders
                .Where(o => !o.IsAssigned)
                .Include(o => o.Account)
                .ToListAsync();
            return View(orders);
        }

        // Shipper nhận đơn hàng
        [HttpPost]
        public async Task<IActionResult> AcceptOrder(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.IsAssigned)
            {
                return NotFound();
            }

            var shipper = await _userManager.GetUserAsync(User);
            if (shipper == null)
            {
                return Unauthorized();
            }

            order.IsAssigned = true;
            order.ShipperId = shipper.Id;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(UnassignedOrders));
        }

        // Hiển thị danh sách đơn hàng mà shipper đã nhận
        public async Task<IActionResult> MyShipping()
        {
            var shipper = await _userManager.GetUserAsync(User);
            if (shipper == null)
            {
                return Unauthorized();
            }

            var orders = await _context.Orders
                .Where(o => o.ShipperId == shipper.Id)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ToListAsync();

            return View(orders);
        }

        // Cập nhật trạng thái từng sản phẩm trong đơn hàng
        [HttpPost]
        public async Task<IActionResult> UpdateOrderDetailStatus(int detailId, string status)
        {
            var orderDetail = await _context.OrderDetails.FindAsync(detailId);
            if (orderDetail == null)
            {
                return NotFound();
            }

            orderDetail.Status = status;
            _context.OrderDetails.Update(orderDetail);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyShipping));
        }
    }
}