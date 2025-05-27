using SweetDream.Models;
using SweetDream.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SweetDream.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly DataContext _context;

        public ShoppingCartController(DataContext context)
        {
            _context = context;
        }

        // Hiển thị giỏ hàng
        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Authentication");
            }

            var userId = GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            var cart = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.AccountId == userId && o.IsCart);

            return View(cart ?? new Order { OrderDetails = new List<OrderDetail>() });
        }

        // Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity, bool redirectToCart = false)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            var cart = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.AccountId == userId && o.IsCart);

            if (cart == null)
            {
                cart = new Order
                {
                    AccountId = userId.Value,
                    IsCart = true,
                    OrderDetails = new List<OrderDetail>()
                };
                _context.Orders.Add(cart);
            }

            var existingItem = cart.OrderDetails.FirstOrDefault(od => od.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var product = _context.Products.Find(productId);
                if (product != null)
                {
                    cart.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        Price = product.Price,
                        Discount = product.Discount ?? 0,
                        OrderDate = DateTime.Now,
                        Status = "Pending"
                    });
                }
            }

            _context.SaveChanges();
            if (redirectToCart)
            {
                return RedirectToAction("Index", "ShoppingCart");
            }
            return Json(new { success = true });
        }
        // Xóa sản phẩm khỏi giỏ hàng
        public IActionResult RemoveFromCart(int productId)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            var cart = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.AccountId == userId && o.IsCart);

            if (cart != null)
            {
                var item = cart.OrderDetails.FirstOrDefault(od => od.ProductId == productId);
                if (item != null)
                {
                    _context.OrderDetails.Remove(item);
                    _context.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }

        // Xóa toàn bộ giỏ hàng
        public IActionResult ClearCart()
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            var cart = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.AccountId == userId && o.IsCart);

            if (cart != null)
            {
                _context.OrderDetails.RemoveRange(cart.OrderDetails);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Checkout(int[] selectedProducts, Order model)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            var cart = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.AccountId == userId && o.IsCart);

            if (cart == null || !cart.OrderDetails.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Index");
            }

            // Lọc sản phẩm được chọn để checkout
            var selectedOrderDetails = cart.OrderDetails
                .Where(od => selectedProducts.Contains(od.ProductId))
                .ToList();

            if (!selectedOrderDetails.Any())
            {
                TempData["Error"] = "No products selected for purchase!";
                return RedirectToAction("Index");
            }

            // Cập nhật thông tin đơn hàng
            cart.ShippingAddress = model.ShippingAddress;
            cart.PhoneNumber = model.PhoneNumber;
            cart.CustomerNote = model.CustomerNote;

            foreach (var item in selectedOrderDetails)
            {
                var product = _context.Products.FirstOrDefault(p => p.ProductId == item.ProductId);
                if (product != null)
                {
                    if (product.Quantity < item.Quantity)
                    {
                        TempData["Error"] = $"Not enough stock for {product.ProductName}. Available: {product.Quantity}";
                        return RedirectToAction("Index");
                    }

                    product.Quantity -= item.Quantity;
                }

                item.Status = "Processing";
                item.OrderDate = DateTime.Now;
            }

            if (selectedOrderDetails.Count == cart.OrderDetails.Count)
            {
                cart.IsCart = false;
            }
            else
            {
                var newOrder = new Order
                {
                    AccountId = userId.Value,
                    ShippingAddress = model.ShippingAddress,
                    PhoneNumber = model.PhoneNumber,
                    CustomerNote = model.CustomerNote,
                    OrderDetails = selectedOrderDetails,
                    IsCart = false
                };
                _context.Orders.Add(newOrder);
                // Xóa các sản phẩm đã checkout khỏi giỏ hàng
                foreach (var item in selectedOrderDetails)
                {
                    cart.OrderDetails.Remove(item);
                }
            }

            _context.SaveChanges();

            TempData["Success"] = "Order placed successfully!";
            return RedirectToAction("OrderConfirmation");
        }

        public IActionResult OrderConfirmation()
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            var latestOrder = _context.Orders
                .Where(o => o.AccountId == userId && !o.IsCart)
                .OrderByDescending(o => o.OrderDetails.Max(od => od.OrderDate))
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault();
            if (latestOrder == null)
            {
                return RedirectToAction("Index");
            }

            return View(latestOrder);
        }

        // Lấy ID người dùng từ Claims
        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;
        }

        [HttpPost]
        public IActionResult ConfirmOrder(int[] selectedProducts)
        {
            var userId = GetUserId();
            if (userId == null)
            {
        return RedirectToAction("Login", "Authentication");
        }

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
        return RedirectToAction("Index", "Home");
        }

        var cart = _context.Orders
        .Include(o => o.OrderDetails)
        .ThenInclude(od => od.Product)
        .FirstOrDefault(o => o.AccountId == user.Id && o.IsCart);

        if (cart == null)
        {
        return RedirectToAction("Index", "ShoppingCart");
        }

        cart.OrderDetails = cart.OrderDetails.Where(od => selectedProducts.Contains(od.ProductId)).ToList();

        if (!cart.OrderDetails.Any())
        {
        TempData["Error"] = "No products selected for purchase!";
        return RedirectToAction("Index");
        }

        return View(cart);
        }


        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                return BadRequest(new { message = "Product not found." });
            }

            if (quantity > product.Quantity)
            {
                return BadRequest(new { message = "Not enough stock available." });
            }

            var cart = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.AccountId == userId && o.IsCart);

            if (cart != null)
            {
                var item = cart.OrderDetails.FirstOrDefault(od => od.ProductId == productId);
                if (item != null)
                {
                    item.Quantity = quantity;
                    _context.SaveChanges();
                    return Ok(new { message = "Cart updated successfully" });
                }
            }

            return BadRequest(new { message = "Cart update failed." });
        }

        [HttpPost]
        public IActionResult CheckStock(int productId, int quantity)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);

            if (product == null)
            {
                return Json(new { success = false, message = "Product not found." });
            }

            if (quantity > product.Quantity)
            {
                return Json(new { success = false, maxQuantity = product.Quantity });
            }

            return Json(new { success = true });
        }
    }
}
