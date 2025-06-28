using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SweetDream.Hubs;
using SweetDream.Models;
using SweetDream.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using X.PagedList;

namespace SweetDream.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class BlogsController : Controller
    {
        private readonly DataContext _context;
        private readonly IHubContext<ProductHub> _hubContext;

        public BlogsController(DataContext context, IHubContext<ProductHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index(int? page, string searchString, BlogStatus? statusFilter)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var blogs = _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.BlogCategory)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                blogs = blogs.Where(b => b.Title.Contains(searchString));
            }

            if (statusFilter.HasValue)
            {
                blogs = blogs.Where(b => b.Status == statusFilter.Value);
            }

            var pagedList = await blogs
                .OrderByDescending(b => b.CreatedAt)
                .ToPagedListAsync(pageNumber, pageSize);

            ViewBag.SearchString = searchString;
            ViewBag.StatusFilter = statusFilter;

            return View(pagedList);
        }

        public IActionResult Create()
        {
            var adminRoleId = _context.Roles.FirstOrDefault(r => r.Name.ToLower() == "admin")?.Id;

            var adminAccounts = _context.Users
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == adminRoleId))
                .ToList();

            ViewData["AuthorId"] = new SelectList(adminAccounts, "Id", "FirstName");
            ViewData["BlogCategoryId"] = new SelectList(_context.BlogCategories, "Id", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,AuthorId,BlogCategoryId,Status")] Blog blog, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
            {
                foreach (var modelError in ModelState)
                {
                    Console.WriteLine($"Key: {modelError.Key}");
                    foreach (var error in modelError.Value.Errors)
                    {
                        Console.WriteLine($" - Error: {error.ErrorMessage}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                blog.CreatedAt = DateTime.Now;

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    blog.ImageUrl = "/img/" + fileName;
                }
                else
                {
                    blog.ImageUrl = "/img/Blog.png";
                }

                _context.Add(blog);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Blog created successfully!";

                await _hubContext.Clients.All.SendAsync("ReceiveBlogUpdate");
                return RedirectToAction(nameof(Index));
            }

            var adminRoleIdReload = _context.Roles.FirstOrDefault(r => r.Name.ToLower() == "admin")?.Id;
            var adminAccountsReload = _context.Users
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == adminRoleIdReload))
                .ToList();

            ViewData["AuthorId"] = new SelectList(adminAccountsReload, "Id", "FirstName", blog.AuthorId);
            ViewData["BlogCategoryId"] = new SelectList(_context.BlogCategories, "Id", "Name", blog.BlogCategoryId);

            return View(blog);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null) return NotFound();

            ViewData["AuthorId"] = new SelectList(_context.Accounts, "Id", "FirstName");
            ViewData["BlogCategoryId"] = new SelectList(_context.BlogCategories, "Id", "Name");

            return View(blog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Blog model, string? status, IFormFile? ImageFile, string ExistingImageUrl)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    model.ImageUrl = "/img/" + fileName;
                }
                else
                {
                    model.ImageUrl = ExistingImageUrl;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrEmpty(status) && Enum.TryParse<BlogStatus>(status, out var statusEnum))
                    {
                        model.Status = statusEnum;
                    }
                    else
                    {
                        ModelState.AddModelError("Status", "Trạng thái không hợp lệ");
                        PrepareViewBagForEdit(model);
                        return View(model);
                    }

                    model.UpdatedAt = DateTime.Now;

                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Blog updated successfully!";
                    await _hubContext.Clients.All.SendAsync("ReceiveBlogUpdate");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(model.Id)) return NotFound();
                    else throw;
                }

                return RedirectToAction(nameof(Index));
            }

            PrepareViewBagForEdit(model);
            return View(model);
        }

        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.Id == id);
        }

        private void PrepareViewBagForEdit(Blog model)
        {
            ViewBag.StatusList = Enum.GetValues(typeof(BlogStatus))
                .Cast<BlogStatus>()
                .Select(s => new SelectListItem
                {
                    Text = s.ToString(),
                    Value = s.ToString(),
                    Selected = (s == model.Status)
                }).ToList();

            ViewBag.BlogCategoryList = _context.BlogCategories
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                    Selected = (c.Id == model.BlogCategoryId)
                }).ToList();
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var blog = await _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.BlogCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (blog == null) return NotFound();

            return View(blog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog != null)
            {
                if (blog.Status == BlogStatus.Expired)
                {
                    _context.Blogs.Remove(blog);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Blog deleted successfully!";
                    await _hubContext.Clients.All.SendAsync("ReceiveBlogUpdate");
                }
                else
                {
                    TempData["ErrorMessage"] = "Chỉ có thể xóa bài viết đã hết hạn.";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}