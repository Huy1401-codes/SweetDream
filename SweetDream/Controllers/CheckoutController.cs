using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<Account> _userManager;
        public CheckoutController(IVnPayService vnPayService,
            DataContext dataContext, EmailService emailService, UserManager<Account> userManager)
        {

            _vnPayService = vnPayService;
            _dataContext = dataContext;
            _emailService = emailService;
            _userManager = userManager;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrderSubmitted(Order order)
        {
            var account = await _userManager.GetUserAsync(User);
            if (account == null || string.IsNullOrEmpty(account.Email))
            {
                TempData["Error"] = "Không lấy được email người dùng.";
                return View(order);
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                TempData["Error"] = "Bạn chưa đăng nhập hoặc phiên đăng nhập đã hết hạn.";
                return RedirectToAction("Login", "Authentication");
            }

            var latestOrder = await _dataContext.Orders
                .Where(o => o.AccountId == userId && !o.IsCart)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OId)
                .FirstOrDefaultAsync();

            if (latestOrder == null || latestOrder.OrderDetails == null || !latestOrder.OrderDetails.Any())
            {
                TempData["Error"] = "Không tìm thấy đơn hàng hợp lệ để xử lý.";
                return View(order);
            }

            // Cập nhật đơn hàng với dữ liệu gửi lên từ form
            latestOrder.ShippingAddress = order.ShippingAddress;
            latestOrder.PhoneNumber = order.PhoneNumber;
            latestOrder.CustomerNote = order.CustomerNote;

            _dataContext.Orders.Update(latestOrder);
            await _dataContext.SaveChangesAsync();

            var cultureVN = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
            var totalAmount = latestOrder.OrderDetails.Sum(od => od.Quantity * (od.Price - od.Discount));

            // Tạo chuỗi chi tiết đơn hàng
            string orderDetailsHtml = "<ul>";
            foreach (var item in latestOrder.OrderDetails)
            {
                orderDetailsHtml += $"<li>{item.Product.ProductName} - Số lượng: {item.Quantity} - Giá: {item.Price.ToString("C0", cultureVN)}</li>";
            }
            orderDetailsHtml += "</ul>";

            // Tạo link xác nhận thanh toán
            var confirmLink = Url.Action("OrderConfirmed", "Checkout", null, protocol: Request.Scheme);

            string body = $@"
    <h3>SweetDream - Xác nhận đơn hàng</h3>
    <p>Xin chào <strong>{account.UserName} {account.FirstName}</strong>,</p>
    <p>Bạn đã đặt đơn hàng với tổng giá trị: <strong>{totalAmount.ToString("C0", cultureVN)}</strong>.</p>
    <p>Chi tiết đơn hàng:</p>
    {orderDetailsHtml}
    <p>Địa chỉ giao hàng: {latestOrder.ShippingAddress}</p>
    <p>Số điện thoại liên hệ: {latestOrder.PhoneNumber}</p>
    <p>Ghi chú: {latestOrder.CustomerNote}</p>
    
    <p style='font-size: 16px; font-weight: bold;'>✅ Xác nhận đã thanh toán đơn hàng mã <span style='color: #E2171A;'>#{latestOrder.OId}</span></p>
    
    <p>Vui lòng ấn vào link bên dưới để xác nhận:</p>

    <p>
        <a href='{confirmLink}' style='display: inline-block; background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
            Xác nhận đơn hàng
        </a>
    </p>

    <p>Trân trọng,<br>SweetDream Team</p>";


            try
            {
                var emailSent = await _emailService.SendEmailAsync(account.Email, "Xác nhận đơn hàng SweetDream", body);
                if (emailSent)
                {
                    TempData["Success"] = "Mail gửi thành công.";
                    return View("OrderSubmitted");
                }
                else
                {
                    TempData["Error"] = "Gửi mail thất bại.";
                    return View(order);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi gửi mail: " + ex.Message;
                return View(order);
            }
        }





        [HttpGet]
        public IActionResult OrderConfirmed()
        {
            return View("OrderConfirmed");
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
