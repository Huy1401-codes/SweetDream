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
                new Brand { BrandName = "Everon", Description = "Thương hiệu chăn ga gối đệm chất lượng cao" },
                new Brand { BrandName = "Liên Á", Description = "Chuyên cung cấp nệm và gối cao su thiên nhiên" },
                new Brand { BrandName = "Vạn Thành", Description = "Đệm và gối Việt Nam uy tín" },
                new Brand { BrandName = "Kim Cương", Description = "Nệm và gối chất lượng cho gia đình Việt" },
                new Brand { BrandName = "Hanvico", Description = "Chăn ga gối cao cấp" },
                new Brand { BrandName = "Sông Hồng", Description = "Thương hiệu quen thuộc với mọi gia đình" },
                new Brand { BrandName = "Kymdan", Description = "Nệm cao su thiên nhiên nổi tiếng" },
                new Brand { BrandName = "Therapedic", Description = "Thương hiệu quốc tế với chất lượng vượt trội" },
                new Brand { BrandName = "Amando", Description = "Chăn ga gối đệm hiện đại" },
                new Brand { BrandName = "Ru9", Description = "Nệm foam thế hệ mới" }
            };

            _context.Brands.AddRange(brands);
            await _context.SaveChangesAsync();
        }


        if (!_context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new Category { CategoryName = "Chăn", Description = "Footwear for various purposes." },
                new Category { CategoryName = "Gối", Description = "Different types of shirts for all occasions." },
                new Category { CategoryName = "Combo", Description = "Various styles of pants." },
                new Category { CategoryName = "Chăn", Description = "Outerwear for all seasons." },
                new Category { CategoryName = "Chăn", Description = "Comfortable wear for warm weather." },
                new Category { CategoryName = "Gối", Description = "Footwear accessories for comfort." },
                new Category { CategoryName = "Gối", Description = "Fashion accessories to complete your look." },
                new Category { CategoryName = "Gối", Description = "Headwear for fashion or function." },
                new Category { CategoryName = "Gối", Description = "Casual wear for comfort." },
                new Category { CategoryName = "Gối", Description = "Handwear for warmth and style." }
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
    new Product { ProductName = "Chăn lông cừu Everon", Quantity = 30, Price = 1500000, BrandId = brands[0].BrandId, CategoryId = categories[0].CategoryId, Size = "180x200", Material = "Lông cừu", Image = "1.png", Discount = 10, Description = "Chăn lông mềm mại, giữ nhiệt tốt cho mùa đông", Status = "Available" },
    new Product { ProductName = "Ga trải giường Hanvico Cotton", Quantity = 25, Price = 950000, BrandId = brands[4].BrandId, CategoryId = categories[1].CategoryId, Size = "160x200", Material = "Cotton 100%", Image = "11.png", Discount = 5, Description = "Ga trải giường mềm mịn, thoáng mát", Status = "Available" },
    new Product { ProductName = "Gối nằm Liên Á cao su thiên nhiên", Quantity = 50, Price = 600000, BrandId = brands[1].BrandId, CategoryId = categories[2].CategoryId, Size = "60x40", Material = "Cao su thiên nhiên", Image = "3.png", Discount = 8, Description = "Gối cao su đàn hồi, hỗ trợ giấc ngủ sâu", Status = "Available" },
    new Product { ProductName = "Gối ôm Sông Hồng dáng dài", Quantity = 40, Price = 250000, BrandId = brands[5].BrandId, CategoryId = categories[3].CategoryId, Size = "100x30", Material = "Bông mềm", Image = "4.png", Discount = 0, Description = "Gối ôm nhẹ, thoáng khí, dễ vệ sinh", Status = "Available" },
    new Product { ProductName = "Bộ chăn ga gối Vạn Thành hoa văn", Quantity = 20, Price = 1800000, BrandId = brands[2].BrandId, CategoryId = categories[4].CategoryId, Size = "180x200", Material = "Cotton Poly", Image = "5.png", Discount = 12, Description = "Bộ sản phẩm sang trọng, tiện lợi", Status = "Available" },
    new Product { ProductName = "Nệm foam Kim Cương 5 Zone", Quantity = 10, Price = 3500000, BrandId = brands[3].BrandId, CategoryId = categories[5].CategoryId, Size = "160x200", Material = "Foam đa tầng", Image = "6.png", Discount = 15, Description = "Nệm nâng đỡ cơ thể, đàn hồi tốt", Status = "Available" },
    new Product { ProductName = "Mền hè Liên Á siêu nhẹ", Quantity = 35, Price = 520000, BrandId = brands[1].BrandId, CategoryId = categories[6].CategoryId, Size = "180x200", Material = "Microfiber", Image = "7.png", Discount = 5, Description = "Mền thoáng khí, nhẹ nhàng cho mùa hè", Status = "Available" },
    new Product { ProductName = "Ruột gối Amando kháng khuẩn", Quantity = 60, Price = 280000, BrandId = brands[8].BrandId, CategoryId = categories[7].CategoryId, Size = "60x40", Material = "Bông kháng khuẩn", Image = "8.png", Discount = 7, Description = "Ruột gối mềm, không gây dị ứng", Status = "Available" },
    new Product { ProductName = "Tấm bảo vệ nệm Everon chống thấm", Quantity = 45, Price = 490000, BrandId = brands[0].BrandId, CategoryId = categories[8].CategoryId, Size = "180x200", Material = "Poly chống thấm", Image = "9.png", Discount = 9, Description = "Bảo vệ nệm khỏi bụi bẩn, ẩm mốc", Status = "Available" },
    new Product { ProductName = "Bộ phụ kiện giường ngủ Kymdan", Quantity = 15, Price = 1250000, BrandId = brands[6].BrandId, CategoryId = categories[9].CategoryId, Size = "Full Combo", Material = "Đa chất liệu", Image = "10.png", Discount = 10, Description = "Bao gồm gối, ga, mền – đủ dùng cho cả gia đình", Status = "Available" }
};

            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();
        }
    }
}
