
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SweetDream.Models;
using SweetDream.Repositories;
using X.PagedList;

namespace SweetDream.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly DataContext _context;

        public CategoriesController(DataContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        public async Task<IActionResult> Search(int id, string sortOrder, int page = 1)
        {
            int pageSize = 6;

            var category = await _context.Categories
                .Include(c => c.Products)
                .ThenInclude(p => p.Brand)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null || category.Disable)
            {
                return NotFound();
            }

            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.CategoryId == id
                            && p.Brand != null
                            && !p.Category.Disable
                            && !p.Brand.Disable
                            && !p.Disable) 
                .AsQueryable();

            ViewData["CurrentSort"] = sortOrder;
            ViewData["Category"] = category;

            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = products.OrderBy(p => p.ProductId);
                    break;
            }

            var pagedProducts = await products.ToPagedListAsync(page, pageSize);

            return View(pagedProducts);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,CategoryName,Description,Disable")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,CategoryName,Description,Disable")] Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Đảo trạng thái Disable
            category.Disable = !category.Disable;

            _context.Update(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}