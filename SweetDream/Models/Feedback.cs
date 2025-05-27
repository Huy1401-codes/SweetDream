using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SweetDream.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }

        [Required(ErrorMessage = "Feedback content is required.")]
        public string Content { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Account ID is required.")]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Product ID is required.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Order Detail ID is required.")]
        [ForeignKey("OrderDetail")]
        public int DetailId { get; set; }

        public bool? Disable { get; set; } = false;

        // Foreign Keys
        [ForeignKey("AccountId")]
        public virtual Account? Account { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("DetailId")]
        public virtual OrderDetail? OrderDetail { get; set; } 
    }
}
