using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SweetDream.Models;
using SweetDream.Repositories;
using X.PagedList;

namespace SweetDream.Controllers
{
    public class BlogsController : Controller
    {

        private readonly DataContext _context;

        public BlogsController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? page, string searchString, string category)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var query = _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.BlogCategory)
                .Where(b => b.Status == BlogStatus.Published)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(b => b.Title.Contains(searchString) || b.Content.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(b => b.BlogCategory.Name == category);
            }

            var pagedList = await query.OrderByDescending(b => b.CreatedAt).ToPagedListAsync(pageNumber, pageSize);

            var categories = await _context.BlogCategories
                .Select(c => new
                {
                    c.Name,
                    Count = c.Blogs.Count(b => b.Status == BlogStatus.Published)
                })
                .ToListAsync();

            ViewBag.Categories = categories;
            // Lấy top 4 blog nhiều like nhất
            var topLiked = _context.Blogs
                .OrderByDescending(b => b.LikeCount)
                .Take(4)
                .ToList();

            ViewBag.TopLikedBlogs = topLiked;
            return View(pagedList);
        }

        public async Task<IActionResult> Details(int id)
        {
            var blog = await _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.BlogCategory)
                .FirstOrDefaultAsync(b => b.Id == id && b.Status == BlogStatus.Published);

            if (blog == null)
                return NotFound();

            // Tìm blog trước đó (Id nhỏ hơn hiện tại)
            var previousBlog = await _context.Blogs
                .Where(b => b.Id < id && b.Status == BlogStatus.Published)
                .OrderByDescending(b => b.Id)
                .FirstOrDefaultAsync();

            // Tìm blog sau đó (Id lớn hơn hiện tại)
            var nextBlog = await _context.Blogs
                .Where(b => b.Id > id && b.Status == BlogStatus.Published)
                .OrderBy(b => b.Id)
                .FirstOrDefaultAsync();

            ViewBag.PreviousBlog = previousBlog;
            ViewBag.NextBlog = nextBlog;

            return View(blog);

        }
        [HttpPost]
        public async Task<IActionResult> Like(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
                return NotFound();

            blog.LikeCount++;
            await _context.SaveChangesAsync();

            return Json(new { likeCount = blog.LikeCount });
        }




    }
}
