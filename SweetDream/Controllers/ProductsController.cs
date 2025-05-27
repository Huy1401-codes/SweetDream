using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SweetDream.Models;
using SweetDream.Repositories;
using X.PagedList;
namespace SweetDream.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DataContext _context;

        public ProductsController(DataContext context)
        {
            _context = context;
        }

        // GET: Products

        public async Task<IActionResult> Index(int? page, string searchString, string statusFilter)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var products = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .AsQueryable();

            // Cập nhật trạng thái sản phẩm dựa vào số lượng
            var productsToUpdate = await products.Where(p => (p.Quantity == 0 && p.Status != "Sold out") ||
                                                              (p.Quantity > 0 && p.Status != "Available")).ToListAsync();

            if (productsToUpdate.Any())
            {
                foreach (var product in productsToUpdate)
                {
                    product.Status = product.Quantity == 0 ? "Sold out" : "Available";
                }
                await _context.SaveChangesAsync();
            }

            // Tìm kiếm theo tên sản phẩm
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.ProductName.Contains(searchString));
            }

            // Lọc theo trạng thái sản phẩm
            if (!string.IsNullOrEmpty(statusFilter))
            {
                products = products.Where(p => p.Status == statusFilter);
            }

            // Phân trang
            var pagedList = await products.OrderBy(p => p.ProductId)
                                          .ToPagedListAsync(pageNumber, pageSize);

            ViewBag.SearchString = searchString;
            ViewBag.StatusFilter = statusFilter;

            return View(pagedList);
        }



        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["BrandId"] = new SelectList(_context.Brands, "BrandId", "BrandName");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,Quantity,Price,BrandId,Material,CategoryId,Size,Discount,Description,Disable")] Product product, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                product.Status = "Available";
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    product.Image = fileName;
                }
                else
                {
                    product.Image = "Product.png";
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["BrandId"] = new SelectList(_context.Brands, "BrandId", "BrandName", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }



        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewData["BrandId"] = new SelectList(_context.Brands, "BrandId", "BrandName", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,Quantity,Price,BrandId,Material,CategoryId,Size,Discount,Description,Disable,Status,Image")] Product product, IFormFile? ImageFile)
        {
            if (id != product.ProductId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id);
                    if (existingProduct == null) return NotFound();

                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var fileName = Path.GetFileName(ImageFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        product.Image = fileName; 
                    }
                    else
                    {
                        product.Image = existingProduct.Image; 
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.ProductId == product.ProductId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["BrandId"] = new SelectList(_context.Brands, "BrandId", "BrandName", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }


        // POST: Products/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
