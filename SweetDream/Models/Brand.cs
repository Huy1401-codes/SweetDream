using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SweetDream.Models
{
    public class Brand
    {
        [Key]
        public int BrandId { get; set; }

        [Required, StringLength(100)]
        public string BrandName { get; set; }

        [StringLength(500)] 
        public string Description { get; set; }

        public bool Disable { get; set; } = false; 

        public ICollection<Product> Products { get; set; } = new List<Product>(); 
    }
}
