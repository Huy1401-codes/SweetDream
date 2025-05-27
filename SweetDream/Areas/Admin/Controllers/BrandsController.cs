using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SweetDream.Models;
using SweetDream.Repositories;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;

namespace SweetDream.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class BrandsController : Controller
    {
        private readonly DataContext _context;

        public BrandsController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var brands = await _context.Brands
                .Where(b => !b.Disable)
                .ToListAsync();

            return View(brands);
        }

        public IActionResult Search(int id, string sortOrder, int page = 1)
        {
            int pageSize = 6;

            var brand = _context.Brands
                .Include(b => b.Products)
                .ThenInclude(p => p.Category)
                .FirstOrDefault(b => b.BrandId == id);

            if (brand == null || brand.Disable)
            {
                return NotFound();
            }

            var products = brand.Products
                .Where(p => p.Category != null && !p.Category.Disable)
                .AsQueryable();

            ViewData["CurrentSort"] = sortOrder;

            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
            }

            var pagedProducts = products.ToPagedList(page, pageSize);
            ViewData["Brand"] = brand;
            return View(pagedProducts);
        }

        public IActionResult Create()
        {
            return View();
        }

        // POST: Brands/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BrandId,BrandName,Description,Disable")] Brand brand)
        {
            if (ModelState.IsValid)
            {
                _context.Add(brand);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Brand added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        // GET: Brands/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }

        // POST: Brands/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BrandId,BrandName,Description,Disable")] Brand brand)
        {
            if (id != brand.BrandId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(brand);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Brand updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BrandExists(brand.BrandId))
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
            return View(brand);
        }

        private bool BrandExists(int id)
        {
            return _context.Brands.Any(e => e.BrandId == id);
        }

        [HttpPost]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            brand.Disable = true;
            _context.Update(brand);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Brand deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
