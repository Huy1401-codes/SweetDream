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
        //Get Blog
        public async Task<IActionResult> Index(int? page, string searchString, BlogStatus? statusFilter)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var blogs = _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.BlogCategory)
                .AsQueryable();

            // Tìm kiếm theo tiêu đề
            if (!string.IsNullOrEmpty(searchString))
            {
                blogs = blogs.Where(b => b.Title.Contains(searchString));
            }

            // Lọc theo trạng thái
            if (statusFilter.HasValue)
            {
                blogs = blogs.Where(b => b.Status == statusFilter.Value);
            }

            // Sắp xếp & phân trang
            var pagedList = await blogs
                .OrderByDescending(b => b.CreatedAt)
                .ToPagedListAsync(pageNumber, pageSize);

            ViewBag.SearchString = searchString;
            ViewBag.StatusFilter = statusFilter;

            return View(pagedList);
        }


        //Create Blog
        public IActionResult Create()
        {
            // Phương thức Create (GET)
            ViewData["AuthorId"] = new SelectList(_context.Accounts, "Id", "FirstName");
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

                // Image
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

                await _hubContext.Clients.All.SendAsync("ReceiveBlogUpdate");
                TempData["SuccessMessage"] = "Blog Create successfully!";
                return RedirectToAction(nameof(Index));
            }

            // Load lại dropdown khi có lỗi
            ViewData["AuthorId"] = new SelectList(_context.Accounts, "Id", "FirstName", blog.AuthorId);
            ViewData["BlogCategoryId"] = new SelectList(_context.BlogCategories, "Id", "Name", blog.BlogCategoryId);


            return View(blog);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null) return NotFound();

            // Phương thức Edit (GET)
            ViewData["AuthorId"] = new SelectList(_context.Accounts, "Id", "FirstName");
            ViewData["BlogCategoryId"] = new SelectList(_context.BlogCategories, "Id", "Name");

            return View(blog);
        }

        // POST: Blogs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Blog model, string? status, IFormFile? ImageFile, string ExistingImageUrl)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Upload ảnh mới
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
                    // Giữ lại ảnh cũ nếu không upload ảnh mới
                    model.ImageUrl = ExistingImageUrl;
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    // Chuyển string status (từ dropdown) về enum
                    if (!string.IsNullOrEmpty(status) && Enum.TryParse<BlogStatus>(status, out var statusEnum))
                    {
                        model.Status = statusEnum;
                    }
                    else
                    {
                        ModelState.AddModelError("Status", "Trạng thái không hợp lệ");
                        // Chuẩn bị lại dữ liệu viewbag nếu lỗi
                        PrepareViewBagForEdit(model);
                        return View(model);
                    }

                    // Cập nhật thời gian chỉnh sửa
                    model.UpdatedAt = DateTime.Now;

                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(model.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }
            TempData["SuccessMessage"] = "Blog Update successfully!";
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

        //Detail Blog
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.BlogCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        //Delete BLog
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
                    await _hubContext.Clients.All.SendAsync("ReceiveProductUpdate");
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
