namespace SweetDream.Models
{
    public class Wishlist
    {
        public int WishlistId { get; set; }
        public int ProductId { get; set; } // Khóa ngoại liên kết với Product
        public int AccountId { get; set; }  // Khóa ngoại liên kết với Account (User)

        public Product Product { get; set; }  // Liên kết tới sản phẩm
        public Account Account { get; set; }  // Liên kết tới tài khoản người dùng (nếu dùng Identity)
    }
}
