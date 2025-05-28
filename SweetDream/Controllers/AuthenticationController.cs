using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SweetDream.Models;
using SweetDream.Services;


namespace SweetDream.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly UserManager<Account> _userManager;
        private readonly SignInManager<Account> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;

        public AuthenticationController(UserManager<Account> userManager, SignInManager<Account> signInManager,
                                        RoleManager<Role> roleManager, IConfiguration config, EmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _config = config;
            _emailService = emailService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null || user.Active == false) 
            {
                ViewBag.Error = "Account is invalid or has been disabled.";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, true, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                if (await _userManager.IsInRoleAsync(user, "ADMIN"))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                if (await _userManager.IsInRoleAsync(user, "SHIPPER"))
                {
                    return RedirectToAction("MyShipping", "Shippers");
                }

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid username or password";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Roles = _roleManager.Roles.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("UserName,FirstName,LastName,Email,PhoneNumber,Address,DateOfBirth,PasswordHash")] Account account, string roleName, string ConfirmPassword)
        {
            if (account.PasswordHash != ConfirmPassword)
            {
                ViewData["ConfirmPasswordError"] = "Passwords do not match.";
                return View(account);
            }

            var existingUser = await _userManager.FindByEmailAsync(account.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "This email is already in use.");
                return View(account);
            }

            if (ModelState.IsValid)
            {
                var token = Guid.NewGuid().ToString();
                account.AccountImage = "User.jpg"; 

                TempData[$"register_{token}"] = Newtonsoft.Json.JsonConvert.SerializeObject(account);
                var confirmationLink = Url.Action("ConfirmEmail", "Authentication", new { token }, Request.Scheme);

                var emailSent = await _emailService.SendEmailAsync(account.Email, "Email Confirmation",
                    $"Click vào link để xác nhận email: <a href='{confirmationLink}'>Xác nhận email</a>");

                if (emailSent)
                {
                    ViewBag.Email = account.Email;
                    return View("CheckEmail");
                }
                else
                {
                    ViewBag.Message = "Failed to send confirmation email.";
                }
            }

            return View(account);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewBag.Message = "Email does not exist.";
                return View();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Authentication", new { token, email = user.Email }, Request.Scheme);

            var emailSent = await _emailService.SendEmailAsync(email, "Password Reset",
                $"Click the link to reset your password: <a href='{resetLink}'>Reset Password</a>");

            if (emailSent)
            {
                ViewBag.Message = "Password reset email has been sent!";
            }
            else
            {
                ViewBag.Message = "Unable to send email.";
            }

            return View();
        }

        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid token or email.");
            }

            ViewBag.Token = token;
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string token, string email, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Invalid email.";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewBag.Error = "User not found.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Password confirmation does not match.";
                return View();
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
            {
                return RedirectToAction("Login");
            }

            ViewBag.Error = string.Join(", ", result.Errors.Select(e => e.Description));
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            if (string.IsNullOrEmpty(token) || !TempData.ContainsKey($"register_{token}"))
            {
                return BadRequest("Invalid email confirmation request.");
            }

            var accountJson = TempData[$"register_{token}"].ToString();
            var account = Newtonsoft.Json.JsonConvert.DeserializeObject<Account>(accountJson);

            var user = new Account
            {
                UserName = account.UserName,
                FirstName = account.FirstName,
                LastName = account.LastName,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                Address = account.Address,
                DateOfBirth = account.DateOfBirth,
                Active = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                AccountImage = account.AccountImage ?? "User.jpg"
            };

            var result = await _userManager.CreateAsync(user, account.PasswordHash);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "CUSTOMER");
                return View("ConfirmEmailSuccess");
            }

            ViewBag.Message = "Failed to create account.";
            return View("Error");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    }

}
