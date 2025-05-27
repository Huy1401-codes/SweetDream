using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SweetDream.Models;
using System.Security.Claims;
using SweetDream.Repositories;

[Authorize]
public class ProfilesController : Controller
{
    private readonly UserManager<Account> _userManager;
    private readonly SignInManager<Account> _signInManager;
    private readonly DataContext _context;

    public ProfilesController(UserManager<Account> userManager, SignInManager<Account> signInManager, DataContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Authentication");
        }
        ViewData["ActivePage"] = "Profile";
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile([Bind("Id,FirstName,LastName,PhoneNumber,Address,Gender,Description,DateOfBirth")] Account model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Authentication");
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PhoneNumber = model.PhoneNumber;
        user.Address = model.Address;
        user.Gender = model.Gender;
        user.Description = model.Description;
        user.DateOfBirth = model.DateOfBirth;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            ViewBag.Success = "Profile updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View("Index", user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfileImage(IFormFile profileImage)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Authentication");
        }

        if (profileImage != null && profileImage.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(profileImage.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(stream);
            }

            user.AccountImage = fileName;
            await _userManager.UpdateAsync(user);
        }

        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Orders(DateTime? startDate, DateTime? endDate, string sortOrder, int pageNumber = 1)
    {
        int pageSize = 5; 

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Authentication");
        }

        var query = _context.OrderDetails
            .Where(od => od.Order.AccountId == user.Id && !od.Order.IsCart)
            .Include(od => od.Order)
            .Include(od => od.Product)
            .AsQueryable();

        // Lọc theo khoảng ngày
        if (startDate.HasValue)
        {
            query = query.Where(od => od.Order.OrderDate >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            endDate = endDate.Value.AddDays(1).AddTicks(-1);
            query = query.Where(od => od.Order.OrderDate <= endDate.Value);
        }

        // Sắp xếp theo OrderDate
        switch (sortOrder)
        {
            case "oldest":
                query = query.OrderBy(od => od.Order.OrderDate);
                break;
            default:
                query = query.OrderByDescending(od => od.Order.OrderDate);
                break;
        }

        var orders = await PaginatedList<OrderDetail>.CreateAsync(query, pageNumber, pageSize);

        ViewData["ActivePage"] = "Orders";
        ViewData["CurrentSort"] = sortOrder;
        ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
        ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");

        return View(orders);
    }


    public async Task<IActionResult> OrderDetails(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Authentication");
        }

        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.OId == id && o.AccountId == user.Id);

        if (order == null || !order.OrderDetails.Any())
        {
            TempData["Error"] = "Không tìm thấy đơn hàng!";
            return RedirectToAction("Orders");
        }

        ViewData["OrderStatus"] = order.OrderDetails.All(od => od.Status == "Success") ? "Success" : "Pending";

        return View(order.OrderDetails);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(int detailId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Authentication");
        }

        var orderDetail = await _context.OrderDetails
            .Include(od => od.Order)
            .FirstOrDefaultAsync(od => od.DetailId == detailId && od.Order.AccountId == user.Id);

        if (orderDetail == null)
        {
            return NotFound();
        }

        if (orderDetail.Status != "Processing")
        {
            TempData["Error"] = "Orders can only be canceled while in Processing status!";
            return RedirectToAction("OrderDetails", new { id = orderDetail.OId });
        }

        orderDetail.Status = "Disabled";
        var allDetails = await _context.OrderDetails.Where(od => od.OId == orderDetail.OId).ToListAsync();
        if (allDetails.All(od => od.Status == "Disabled"))
        {
            orderDetail.Order.IsCart = false;
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Order was successfully canceled!";
        return RedirectToAction("OrderDetails", new { id = orderDetail.OId });
    }
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Authentication");
        }

        if (newPassword != confirmPassword)
        {
            ModelState.AddModelError("", "New password and confirmation password do not match.");
            return View();
        }

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Password changed successfully!";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
        ViewData["ActivePage"] = "ChangePassword";
        return View();
    }
}
