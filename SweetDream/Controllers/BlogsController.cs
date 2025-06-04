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

            return View(pagedList);
        }
        
        public IActionResult Details(int id)
        {
            return View();
        }


    }
}
