using Microsoft.AspNetCore.Mvc;
using SweetDream.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SweetDream.ViewComponents
{
    public class CategoriesViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;

        public CategoriesViewComponent(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IViewComponentResult> InvokeAsync() => View(await _dataContext.Categories.ToListAsync());
        
    }
}
