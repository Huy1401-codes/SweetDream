using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SweetDream.Hubs;
using SweetDream.Models;
using SweetDream.Repositories;

namespace SweetDream.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class BLogCategoriesController : Controller
    {
        private readonly DataContext _context;
        private readonly IHubContext<ProductHub> _hubContext;

        public BLogCategoriesController(DataContext context, IHubContext<ProductHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: Admin/BLogCategories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.BlogCategories.ToListAsync();
            return View(categories);
        }

        // GET: Admin/BLogCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.BlogCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null) return NotFound();

            return View(category);
        }

        // GET: Admin/BLogCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/BLogCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] BlogCategory category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Blog category create successfully!";

                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: Admin/BLogCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.BlogCategories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Admin/BLogCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] BlogCategory category)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Blog category update successfully!";

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogCategoryExists(category.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: Admin/BLogCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.BlogCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Admin/BLogCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.BlogCategories
                .Include(c => c.Blogs)  // load blog liên quan
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Blog category not found.";
                return RedirectToAction(nameof(Index));
            }

            if (category.Blogs.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete category because it has related blogs.";
                return RedirectToAction(nameof(Index));
            }

            _context.BlogCategories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Blog category deleted successfully!";
            return RedirectToAction(nameof(Index));
        }


        private bool BlogCategoryExists(int id)
        {
            return _context.BlogCategories.Any(e => e.Id == id);
        }
    }
}
