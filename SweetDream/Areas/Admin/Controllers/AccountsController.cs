using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SweetDream.Models;
using SweetDream.Repositories;
using Microsoft.AspNetCore.Identity;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;

namespace SweetDream.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AccountsController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<Account> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public AccountsController(DataContext context, UserManager<Account> userManager, RoleManager<Role> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin/Accounts
        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 5; 
            int pageNumber = page ?? 1; 

            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, string>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id.ToString()] = roles.Any() ? string.Join(", ", roles) : "No Role";
            }

            ViewBag.UserRoles = userRoles;
            var pagedList = users.ToPagedList(pageNumber, pageSize);
            return View(pagedList);
        }

        // GET: Admin/Accounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // GET: Admin/Accounts/Create
        public IActionResult Create()
        {
            ViewBag.Roles = _roleManager.Roles.ToList(); 
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Address,Gender,Active,Description,DateOfBirth,Disable,UserName,Email,PhoneNumber,PasswordHash")] Account account, string roleName)
        {
            if (await _userManager.FindByNameAsync(account.UserName) != null)
            {
                ModelState.AddModelError("UserName", "Username already exists.");
            }
            if (await _userManager.FindByEmailAsync(account.Email) != null)
            {
                ModelState.AddModelError("Email", "Email already exists.");
            }

            if (ModelState.IsValid)
            {
                account.AccountImage = "User.jpg";
                var passwordHasher = new PasswordHasher<Account>();
                account.PasswordHash = passwordHasher.HashPassword(account, account.PasswordHash);
                account.AccessFailedCount = 0;

                var result = await _userManager.CreateAsync(account);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(roleName) && await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _userManager.AddToRoleAsync(account, roleName);
                    }
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            ViewBag.Roles = _roleManager.Roles.ToList();
            return View(account);
        }




        // GET: Admin/Accounts/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _userManager.FindByIdAsync(id.ToString());
            if (account == null)
            {
                return NotFound();
            }

            // Lấy danh sách Role
            ViewBag.Roles = _roleManager.Roles.ToList();

            // Lấy Role hiện tại của User
            var userRoles = await _userManager.GetRolesAsync(account);
            ViewBag.CurrentRole = userRoles.FirstOrDefault();

            return View(account);
        }

        // POST: Admin/Accounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserName,FirstName,LastName,Email,PhoneNumber,Address,DateOfBirth,Active,PasswordHash")] Account account, string roleName)
        {
            if (id != account.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingAccount = await _userManager.FindByIdAsync(id.ToString());
                if (existingAccount == null)
                {
                    return NotFound();
                }

                existingAccount.UserName = account.UserName;
                existingAccount.FirstName = account.FirstName;
                existingAccount.LastName = account.LastName;
                existingAccount.Email = account.Email;
                existingAccount.PhoneNumber = account.PhoneNumber;
                existingAccount.Address = account.Address;
                existingAccount.DateOfBirth = account.DateOfBirth;
                existingAccount.Active = account.Active;

                if (!string.IsNullOrEmpty(account.PasswordHash))
                {
                    var passwordHasher = new PasswordHasher<Account>();
                    existingAccount.PasswordHash = passwordHasher.HashPassword(existingAccount, account.PasswordHash);
                }

                // Cập nhật Role nếu thay đổi
                var currentRoles = await _userManager.GetRolesAsync(existingAccount);
                if (!string.IsNullOrEmpty(roleName) && !currentRoles.Contains(roleName))
                {
                    await _userManager.RemoveFromRolesAsync(existingAccount, currentRoles);
                    await _userManager.AddToRoleAsync(existingAccount, roleName);
                }

                var result = await _userManager.UpdateAsync(existingAccount);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            ViewBag.Roles = _roleManager.Roles.ToList();
            return View(account);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfileImage(int id, IFormFile file)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            if (file != null && file.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(user.AccountImage))
                {
                    string oldFilePath = Path.Combine(uploadsFolder, user.AccountImage);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                user.AccountImage = uniqueFileName;
                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction(nameof(Edit), new { id });
        }




        // GET: Admin/Accounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Admin/Accounts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var account = await _context.Users.FindAsync(id);
            if (account == null)
            {
                return Json(new { success = false, message = "Account not found." });
            }

            _context.Users.Remove(account);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Account deleted successfully." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActiveStatus(int id, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            user.Active = isActive;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = "User status updated successfully." });
            }

            return Json(new { success = false, message = "Failed to update user status." });
        }

    }
}
