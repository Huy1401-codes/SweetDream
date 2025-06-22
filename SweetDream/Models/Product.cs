using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SweetDream.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
        public string ProductName { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative number.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public int Price { get; set; }

        [Required(ErrorMessage = "Brand is required.")]
        public int BrandId { get; set; }

        public string Image { get; set; } = "Product.png";

        [StringLength(50, ErrorMessage = "Material name cannot exceed 50 characters.")]
        public string Material { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Size is required.")]
        [StringLength(10, ErrorMessage = "Size cannot exceed 10 characters.")]
        public string Size { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100%.")]
        public int? Discount { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Description { get; set; } = string.Empty;

        public bool Disable { get; set; } = false;

        [Required]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        public string Status { get; set; } = "Available";

        [ForeignKey("BrandId")]
        public virtual Brand? Brand { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public virtual ICollection<OrderDetail>? OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
