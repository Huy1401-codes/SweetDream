namespace SweetDream.Models
{
    public class BlogCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<Blog> Blogs { get; set; } = new List<Blog>();
    }
}
