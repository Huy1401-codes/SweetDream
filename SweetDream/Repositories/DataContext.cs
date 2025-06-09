using SweetDream.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SweetDream.Repositories
{
    public class DataContext : IdentityDbContext<Account, Role, int>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; } = default!;
        public DbSet<Blog> Blogs { get; set; } = default!;
        public DbSet<BlogCategory> BlogCategories { get; set; } = default!;
        public DbSet<Account> Accounts { get; set; }

        public DbSet<VnpayModel> VnInfos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình quan hệ giữa Product và Brand
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình quan hệ giữa Product và Category
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình quan hệ giữa OrderDetail và Product
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình quan hệ giữa OrderDetail và Order
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình quan hệ giữa Account và Role (thông qua bảng trung gian IdentityUserRole<int>)
            modelBuilder.Entity<IdentityUserRole<int>>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // Cấu hình quan hệ giữa Feedback và Account
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Account)
                .WithMany()
                .HasForeignKey(f => f.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình quan hệ giữa Feedback và Product
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Product)
                .WithMany()
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình quan hệ giữa Feedback và OrderDetail
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.OrderDetail)
                .WithMany(od => od.Feedbacks)
                .HasForeignKey(f => f.DetailId)
                .OnDelete(DeleteBehavior.Restrict);
            // Cấu hình quan hệ giữa Wishlist và Product
            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Product)
                .WithMany()  // Một sản phẩm có thể xuất hiện trong nhiều wishlist của các user
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa wishlist nếu sản phẩm bị xóa

            // Cấu hình quan hệ giữa Wishlist và Account (User)
            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Account) // Liên kết với Account (User)
                .WithMany() // Một người dùng có thể có nhiều sản phẩm trong wishlist
                .HasForeignKey(w => w.AccountId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa wishlist nếu tài khoản bị xóa
            modelBuilder.Entity<Blog>()
    .HasOne(b => b.Author)
    .WithMany()
    .HasForeignKey(b => b.AuthorId)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Blog>()
                .HasOne(b => b.BlogCategory)
                .WithMany(c => c.Blogs)
                .HasForeignKey(b => b.BlogCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

        }

        public DbSet<SweetDream.Models.Banner> Banner { get; set; } = default!;
    }
}
