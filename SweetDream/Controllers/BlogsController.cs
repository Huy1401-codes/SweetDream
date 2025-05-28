using Microsoft.AspNetCore.Mvc;

namespace SweetDream.Controllers
{
    public class BlogsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
