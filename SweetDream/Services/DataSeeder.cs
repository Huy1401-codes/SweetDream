using SweetDream.Models;
using SweetDream.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class DataSeeder
{
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<Account> _userManager;
    private readonly DataContext _context;

    public DataSeeder(RoleManager<Role> roleManager, UserManager<Account> userManager, DataContext context)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
    }

    public async Task SeedRolesAndAdminAsync()
    {
        string[] roles = { "Admin", "Staff", "Customer", "Shipper" };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new Role { Name = role });
            }
        }

        // Tạo tài khoản Admin
        await CreateUserIfNotExists("admin@example.com", "Admin@123", "Admin");

        // Tạo tài khoản Shipper
        await CreateUserIfNotExists("shipper@example.com", "@1", "Shipper");
    }

    private async Task CreateUserIfNotExists(string email, string password, string role)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new Account { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
            }
        }
    }

    public async Task SeedBrandsCategoriesProductsAsync()
    {
        if (!_context.Brands.Any())
        {
            var brands = new List<Brand>
            {
                new Brand { BrandName = "Nike", Description = "No description available" },
                new Brand { BrandName = "Adidas", Description = "No description available" },
                new Brand { BrandName = "Puma", Description = "No description available" },
                new Brand { BrandName = "Reebok", Description = "No description available" },
                new Brand { BrandName = "Under Armour", Description = "No description available" },
                new Brand { BrandName = "New Balance", Description = "No description available" },
                new Brand { BrandName = "Fila", Description = "No description available" },
                new Brand { BrandName = "ASICS", Description = "No description available" },
                new Brand { BrandName = "Champion", Description = "No description available" },
                new Brand { BrandName = "Converse", Description = "No description available" }
            };

            _context.Brands.AddRange(brands);
            await _context.SaveChangesAsync();
        }


        if (!_context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new Category { CategoryName = "Shoes", Description = "Footwear for various purposes." },
                new Category { CategoryName = "Shirts", Description = "Different types of shirts for all occasions." },
                new Category { CategoryName = "Pants", Description = "Various styles of pants." },
                new Category { CategoryName = "Jackets", Description = "Outerwear for all seasons." },
                new Category { CategoryName = "Shorts", Description = "Comfortable wear for warm weather." },
                new Category { CategoryName = "Socks", Description = "Footwear accessories for comfort." },
                new Category { CategoryName = "Accessories", Description = "Fashion accessories to complete your look." },
                new Category { CategoryName = "Hats", Description = "Headwear for fashion or function." },
                new Category { CategoryName = "Sweatshirts", Description = "Casual wear for comfort." },
                new Category { CategoryName = "Gloves", Description = "Handwear for warmth and style." }
            };

            _context.Categories.AddRange(categories);
            await _context.SaveChangesAsync();
        }

        if (!_context.BlogCategories.Any())
        {
            var blogcate = new List<BlogCategory>
    {
        new BlogCategory { Name = "Ngủ Trưa Vàng: Trẻ Phát Triển" },
        new BlogCategory { Name = "Gối Tự Nhiên: Giấc Ngủ Bé An Toàn" },
        new BlogCategory { Name = "Tạo Môi Trường Ngủ Trưa Lý Tưởng" },
        new BlogCategory { Name = "Giấc Ngủ Trẻ: Khoa Học & Lợi Ích" },
        new BlogCategory { Name = "Bí Quyết Nâng Cao Chất Lượng Giấc Ngủ Trẻ" }
    };

            _context.BlogCategories.AddRange(blogcate);
            await _context.SaveChangesAsync();
        }

        if (!_context.Users.Any(u => u.UserName == "marketing"))
        {
            var hasher = new PasswordHasher<Account>();

            var marketingAccount = new Account
            {
                UserName = "marketing",
                NormalizedUserName = "MARKETING",
                Email = "marketing@sweetdream.com",
                NormalizedEmail = "MARKETING@SWEETDREAM.COM",
                EmailConfirmed = true,
                PhoneNumber = "0123456789",
                PhoneNumberConfirmed = true,
                FirstName = "Marketing",
                LastName = "SweetDream",
                Address = "123 Đường Giấc Mơ, TP HCM",
                Gender = "Nữ",
                AccountImage = "/images/users/marketing.png",
                Active = true,
                Description = "Tài khoản chuyên phụ trách mảng marketing",
                DateOfBirth = "1995-01-01",
                Disable = false,
                CreatedAt = DateTime.Now,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            marketingAccount.PasswordHash = hasher.HashPassword(marketingAccount, "Marketing@123"); // Đặt mật khẩu mặc định

            _context.Users.Add(marketingAccount);
            await _context.SaveChangesAsync();
        }

        if (!_context.Banner.Any())
        {
            var banners = new List<Banner>
                {
                    new Banner
                    {
                        Title = "Spring Sale",
                        Subtitle = "Up to 50% off",
                        Description = "Hurry up! Limited time offer on all spring collection.",
                        Image = "B1.jpeg",
                        LinkUrl = "/sale/spring"
                    },
                    new Banner
                    {
                        Title = "New Arrivals",
                        Subtitle = "Check out the latest collection",
                        Description = "Fresh styles, fresh looks, fresh you.",
                        Image = "B2.jpeg",
                        LinkUrl = "/new-arrivals"
                    },
                    new Banner
                    {
                        Title = "Exclusive Discount",
                        Subtitle = "Special offers for members",
                        Description = "Get 20% off on your next order with membership.",
                        Image = "B3.jpeg",
                        LinkUrl = "/exclusive-offers"
                    }
                };

            _context.Banner.AddRange(banners);
            await _context.SaveChangesAsync();
        }

        if (!_context.Products.Any())
        {
            var brands = await _context.Brands.ToListAsync();
            var categories = await _context.Categories.ToListAsync();

            var products = new List<Product>
            {
                new Product { ProductName = "Nike Air Max", Quantity = 50, Price = 120, BrandId = brands[0].BrandId, CategoryId = categories[0].CategoryId, Size = "M", Material = "Leather", Image = "P1.jpeg", Discount = 10, Description = "Comfortable running shoes", Status = "Available" },
                new Product { ProductName = "Adidas Ultraboost", Quantity = 40, Price = 140, BrandId = brands[1].BrandId, CategoryId = categories[0].CategoryId, Size = "L", Material = "Mesh", Image = "P10.png", Discount = 5, Description = "Best for sports", Status = "Available" },
                new Product { ProductName = "Puma RS-X", Quantity = 30, Price = 110, BrandId = brands[2].BrandId, CategoryId = categories[0].CategoryId, Size = "S", Material = "Synthetic", Image = "P2.jpeg", Discount = 15, Description = "Stylish and modern", Status = "Available" },
                new Product { ProductName = "Reebok Classic", Quantity = 25, Price = 90, BrandId = brands[3].BrandId, CategoryId = categories[0].CategoryId, Size = "M", Material = "Leather", Image = "P3.jpeg", Discount = 0, Description = "Classic design", Status = "Available" },
                new Product { ProductName = "Under Armour HOVR", Quantity = 35, Price = 130, BrandId = brands[4].BrandId, CategoryId = categories[0].CategoryId, Size = "L", Material = "Foam", Image = "P4.jpeg", Discount = 20, Description = "Great for training", Status = "Available" },
                new Product { ProductName = "New Balance 574", Quantity = 20, Price = 100, BrandId = brands[5].BrandId, CategoryId = categories[0].CategoryId, Size = "S", Material = "Suede", Image = "P5.jpeg", Discount = 5, Description = "Casual wear", Status = "Available" },
                new Product { ProductName = "Fila Disruptor", Quantity = 15, Price = 95, BrandId = brands[6].BrandId, CategoryId = categories[0].CategoryId, Size = "M", Material = "Rubber", Image = "P6.jpeg", Discount = 10, Description = "Chunky sneakers", Status = "Available" },
                new Product { ProductName = "ASICS Gel-Kayano", Quantity = 28, Price = 150, BrandId = brands[7].BrandId, CategoryId = categories[0].CategoryId, Size = "L", Material = "Gel", Image = "P7.jpeg", Discount = 8, Description = "Top-tier running shoes", Status = "Available" },
                new Product { ProductName = "Champion Rally Pro", Quantity = 12, Price = 85, BrandId = brands[8].BrandId, CategoryId = categories[0].CategoryId, Size = "M", Material = "Canvas", Image = "P8.jpeg", Discount = 12, Description = "High-top comfort", Status = "Available" },
                new Product { ProductName = "Converse Chuck Taylor", Quantity = 50, Price = 70, BrandId = brands[9].BrandId, CategoryId = categories[0].CategoryId, Size = "S", Material = "Textile", Image = "P9.jpeg", Discount = 0, Description = "Classic sneaker", Status = "Available" },
                new Product { ProductName = "Nike ZoomX", Quantity = 50, Price = 135, BrandId = brands[0].BrandId, CategoryId = 1, Size = "L", Material = "Mesh", Image = "product1.jpg", Discount = 15, Description = "Lightweight running shoes", Status = "Available" },
                new Product { ProductName = "Adidas NMD", Quantity = 45, Price = 150, BrandId = brands[1].BrandId, CategoryId = 2, Size = "M", Material = "Textile", Image = "product2.jpg", Discount = 10, Description = "Urban sneakers", Status = "Available" },
                new Product { ProductName = "Puma Future Rider", Quantity = 30, Price = 110, BrandId = brands[2].BrandId, CategoryId = 3, Size = "S", Material = "Synthetic", Image = "product3.jpg", Discount = 5, Description = "Casual sneakers", Status = "Available" },
                new Product { ProductName = "Reebok Nano X", Quantity = 20, Price = 120, BrandId = brands[3].BrandId, CategoryId = 4, Size = "L", Material = "Mesh", Image = "product4.jpeg", Discount = 10, Description = "Training shoes", Status = "Available" },
                new Product { ProductName = "Under Armour Curry", Quantity = 25, Price = 140, BrandId = brands[4].BrandId, CategoryId = 5, Size = "M", Material = "Leather", Image = "product5.jpeg", Discount = 5, Description = "Basketball shoes", Status = "Available" },
                new Product { ProductName = "New Balance 990", Quantity = 30, Price = 180, BrandId = brands[5].BrandId, CategoryId = 6, Size = "L", Material = "Suede", Image = "product6.jpeg", Discount = 20, Description = "Premium sneakers", Status = "Available" },
                new Product { ProductName = "Fila Ray Tracer", Quantity = 18, Price = 110, BrandId = brands[6].BrandId, CategoryId = 7, Size = "S", Material = "Synthetic", Image = "product7.jpeg", Discount = 15, Description = "Sporty look", Status = "Available" },
                new Product { ProductName = "ASICS Gel-Lyte III", Quantity = 40, Price = 130, BrandId = brands[7].BrandId, CategoryId = 8, Size = "M", Material = "Gel", Image = "product8.jpeg", Discount = 5, Description = "Running shoes with great stability", Status = "Available" },
                new Product { ProductName = "Champion Reverse Weave", Quantity = 35, Price = 80, BrandId = brands[8].BrandId, CategoryId = 9, Size = "M", Material = "Canvas", Image = "product9.jpeg", Discount = 0, Description = "Classic sweatshirt", Status = "Available" },
                new Product { ProductName = "Converse All Star", Quantity = 45, Price = 75, BrandId = brands[9].BrandId, CategoryId = 10, Size = "L", Material = "Canvas", Image = "product10.jpeg", Discount = 0, Description = "Iconic sneakers", Status = "Available" }
            };


            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();
        }
    }
}
