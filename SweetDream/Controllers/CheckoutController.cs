using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Newtonsoft.Json;
using SweetDream.Models;
using SweetDream.Repositories;
using SweetDream.Services.VnPay;
using System.Collections.Generic;
using System.Security.Claims;

namespace SweetDream.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IVnPayService _vnPayService;
        public CheckoutController(IVnPayService vnPayService, DataContext dataContext)
        {

            _vnPayService = vnPayService;
            _dataContext = dataContext;
        }


        //public async Task<IActionResult> Checkout(string PaymentMethod, string PaymentId)
        //{

        //    var userEmail = User.FindFirstValue(ClaimTypes.Email);

        //    if (userEmail == null)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    else

        //    {

        //        var ordercode = Guid.NewGuid().ToString();

        //        var orderItem = new OrderModel();

        //        orderItem.OrderCode = ordercode;

        //        // Get shipping price from cookie

        //        var shippingPriceCookie = Request.Cookies["ShippingPrice"];

        //        decimal shippingPrice = 0;

        //        //Get Coupon code from cookie

        //        var coupon_code = Request.Cookies["CouponTitle"];

        //        if (shippingPriceCookie != null)
        //        {
        //            var shippingPriceJson = shippingPriceCookie;

        //            shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);


        //        }
        //        else
        //        {
        //            shippingPrice = 0;
        //        }

        //        orderItem.ShippingCost = shippingPrice;
        //        orderItem.CouponCode = coupon_code;
        //        orderItem.UserName = userEmail;
        //        if (PaymentMethod == "COD")
        //        {
        //            orderItem.PaymentMethod = PaymentMethod;
        //        }
        //        else if (PaymentMethod == "VnPay")
        //        {
        //            orderItem.PaymentMethod = "VnPay" + PaymentId;
        //        }
        //        orderItem.Status = 1;
        //        orderItem.CreatedDate = DateTime.Now;
        //        _dataContext.Add(orderItem);
        //        _dataContext.SaveChanges();

        //        //List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>

        //        // foreach (var cart in cartItems)

        //        //{

        //        //    var orderdetail = new OrderDetail();

        //        //    orderdetail.UserName = userEmail;

        //        //    orderdetail.OrderCode = ordercode;

        //        //    orderdetail.ProductId = cart.ProductId;

        //        //    orderdetail.Price = cart.Price;

        //        //    orderdetail.Quantity = cart.Quantity;

        //        //    //update product quantity

        //        //    var product = await _dataContext.Products.Where(p => p.ProductId == cart.ProductId);

        //        //    product.Quantity -= cart.Quantity;

        //        //    product.Sold += cart.Quantity;

        //        //    _dataContext.Update(product);

        //        //    //++update product quantity

        //        //    _dataContext.Add(orderdetail);

        //        //    _dataContext.SaveChanges();

        //        //}

        //        //HttpContext.Session.Remove("Cart");

        //        ////Send mail order when success

        //        //var receiver = userEmail;

        //        //var subject = "Order successful";

        //        //var message = "Order successful, enjoy the service.";

        //        //await _emailSender.SendEmailAsync(receiver, subject, message);
        //    }
        //}

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

                //Proceed to place order when momo payment is successful

                //await Checkout();

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
