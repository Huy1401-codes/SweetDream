using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SweetDream.Models
{
    public class OrderDetail
    {
        [Key]
        public int DetailId { get; set; }

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [Required]
        [ForeignKey("Order")]
        public int OId { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Price must be a positive number")]
        public int Price { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Range(0, 100, ErrorMessage = "Discount must be between 0% and 100%")]
        public int Discount { get; set; } = 0;

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime OrderDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? ReceiveDate { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        public bool IsReviewed { get; set; } = false;

        public virtual Product Product { get; set; }

        public virtual Order Order { get; set; }

        // Thêm danh sách Feedbacks
        public virtual ICollection<Feedback>? Feedbacks { get; set; } = new List<Feedback>();
    }
}
