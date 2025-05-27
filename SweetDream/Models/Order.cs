using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SweetDream.Models
{
    public class Order
    {
        [Key]
        public int OId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public bool IsCart { get; set; } = true;

        [Required(ErrorMessage = "Shipping Address is required.")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } = string.Empty;
        public string? CustomerNote { get; set; }
        public bool IsAssigned { get; set; } = false;
        [ForeignKey("Shipper")]
        public int? ShipperId { get; set; }
        
        [Required]
        [ForeignKey("Account")]
        public int AccountId { get; set; }

        public virtual Account Account { get; set; } // Thông tin người đặt hàng
        public virtual Account? Shipper { get; set; } // Thông tin shipper

        public virtual ICollection<OrderDetail>? OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
