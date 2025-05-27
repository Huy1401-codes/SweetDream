using System.ComponentModel.DataAnnotations;

namespace SweetDream.Models
{
    public class Banner
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Subtitle { get; set; }

        public string Description { get; set; }

        public string Image { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = string.Empty;
    }
}