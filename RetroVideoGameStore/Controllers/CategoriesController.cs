using Microsoft.AspNetCore.Mvc;
using RetroVideoGameStore.Models;


namespace RetroVideoGameStore.Controllers
{
    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            // Use Category model to create fake list of 10 categories
            // Create empty list of categories
            var categories = new List<Category>();
            // Loop to create 10 categories
            for (int i =1; i <= 10; i ++)
            {
                categories.Add(new Category
                {
                    Id = i,
                    Name = "Category " + i.ToString()
                });
            }
            // Pass the list to the view for display
            return View(categories);
        }
        public IActionResult Browse(string categoryName)
        {
            // Grab the category name passed in with the URL
            ViewBag.categoryName = categoryName;
            return View();
        }
    }
}
