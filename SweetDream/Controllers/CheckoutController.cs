using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SweetDream.Models;
using SweetDream.Repositories;
using SweetDream.Services;
using SweetDream.Services.VnPay;
using System.Collections.Generic;
using System.Security.Claims;

namespace SweetDream.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IVnPayService _vnPayService;
        private readonly EmailService _emailService;

        public CheckoutController(IVnPayService vnPayService, DataContext dataContext, EmailService emailService)
        {

            _vnPayService = vnPayService;
            _dataContext = dataContext;
            _emailService = emailService;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrderSubmitted(Order order)
        {
            if (!ModelState.IsValid)
                return View(order);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Lấy đơn hàng mới nhất của user (IsCart = false)
            var latestOrder = await _dataContext.Orders
                .Where(o => o.AccountId == userId && o.IsCart == false)
                .OrderByDescending(o => o.OrderDetails.Max(od => od.OrderDate))
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync();

            if (latestOrder == null || latestOrder.OrderDetails == null || !latestOrder.OrderDetails.Any())
            {
                TempData["Error"] = "Không tìm thấy đơn hàng để xử lý!";
                return RedirectToAction("ShoppingCart", "Cart");
            }

            // Cập nhật thông tin đơn hàng từ form nhập
            latestOrder.ShippingAddress = order.ShippingAddress;
            latestOrder.PhoneNumber = order.PhoneNumber;
            latestOrder.CustomerNote = order.CustomerNote;

            _dataContext.Orders.Update(latestOrder);
            await _dataContext.SaveChangesAsync();

            // Tính tổng tiền đơn hàng
            var cultureVN = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
            var totalAmount = latestOrder.OrderDetails.Sum(od => od.Quantity * (od.Price - od.Discount));

            // Tạo link xác nhận đơn hàng
            var token = Guid.NewGuid().ToString();
            TempData[$"confirm_order_{token}"] = latestOrder.OId;
            var confirmLink = Url.Action("ConfirmOrder", "Checkout", new { token = token }, Request.Scheme);

            // Lấy thông tin user để gửi mail
            var account = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            string body = $@"
        <h3>SweetDream - Xác nhận đơn hàng</h3>
        <p>Xin chào <strong>{account.LastName} {account.FirstName}</strong>,</p>
        <p>Bạn đã đặt đơn hàng với tổng giá trị: <strong>{totalAmount.ToString("C0", cultureVN)}</strong>.</p>
        <p>Vui lòng <a href='{confirmLink}'>nhấn vào đây để xác nhận đơn hàng</a>.</p>
    ";

            await _emailService.SendEmailAsync(account.Email, "Xác nhận đơn hàng SweetDream", body);

            TempData["Success"] = "Đơn hàng đã được tạo. Vui lòng kiểm tra email để xác nhận.";
            return RedirectToAction("ConfirmOrder", new { id = latestOrder.OId });
        }






        [HttpGet]
        public async Task<IActionResult> ConfirmOrder(string token)
        {
            if (!TempData.TryGetValue($"confirm_order_{token}", out var orderIdObj))
                return NotFound();

            int orderId = (int)orderIdObj;

            var order = await _dataContext.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Account)
                .FirstOrDefaultAsync(o => o.OId == orderId);

            if (order == null)
                return NotFound();

            TempData["Success"] = "Xác nhận đơn hàng thành công!";

            return RedirectToAction("OrderSubmitted", new { id = orderId });
        }



        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response.VnPayResponseCode == "00")
            {
                var newVnpayInsert = new VnpayModel
                {
                    OrderId = response.OrderId,

                    PaymentMethod = response.PaymentMethod,

                    OrderDescription = response.OrderDescription,

                    TransactionId = response.TransactionId,

                    PaymentId = response.PaymentId,


                };

                _dataContext.Add(newVnpayInsert);

                await _dataContext.SaveChangesAsync();            

            }
            else
            {
                TempData["success"] = "Vnpay transaction successful.";

                return RedirectToAction("Index", "Cart");
            }

            //return Json(response);
            return View(response);
        }
    }
}
