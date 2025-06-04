namespace SweetDream.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Blog
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [MaxLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; } = null!;

        [Url(ErrorMessage = "Đường dẫn ảnh không hợp lệ")]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public int LikeCount { get; set; } = 0;
        public string? ShortDescription { get; set; } 

        //// Foreign keys
        [Required]
        public int AuthorId { get; set; }

        public Account? Author { get; set; }

        public BlogStatus Status { get; set; }


        [Required]
        public int BlogCategoryId { get; set; }

        public BlogCategory? BlogCategory { get; set; }
    }

}
