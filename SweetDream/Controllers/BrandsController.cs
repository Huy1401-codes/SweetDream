using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SweetDream.Models;
using SweetDream.Repositories;
using X.PagedList;

namespace SweetDream.Controllers
{
    public class BrandsController : Controller
    {
        private readonly DataContext _context;

        public BrandsController(DataContext context)
        {
            _context = context;
        }

        // GET: Brands
        public async Task<IActionResult> Index()
        {
            return View(await _context.Brands.ToListAsync());
        }
        // GET: Brands/Search/5
        public async Task<IActionResult> Search(int id, string searchTerm, string sortOrder, int page = 1)
        {
            int pageSize = 6;

            var brand = await _context.Brands
                .FirstOrDefaultAsync(b => b.BrandId == id);

            if (brand == null || brand.Disable)
            {
                return NotFound();
            }

            var products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.BrandId == id
                            && p.Category != null
                            && !p.Category.Disable
                            && !p.Disable) 
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => p.ProductName.Contains(searchTerm));
            }

            ViewData["CurrentSort"] = sortOrder;
            ViewData["SearchTerm"] = searchTerm;
            ViewData["Brand"] = brand;

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
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            brand.Disable = !brand.Disable;
            _context.Update(brand);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}