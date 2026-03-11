using Microsoft.AspNetCore.Mvc;
using RetroVideoGameStore.Data;

namespace RetroVideoGameStore.Controllers
{
    public class ShopController : Controller
    {
        // dB connection
        private readonly ApplicationDbContext _context;

        // Connect to dB whenever this controller is used
        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            // Get list of categories
            var categories = _context.Categories.OrderBy(c => c.Name).ToList();
            return View(categories);
        }

        // Shop/Browse/3
        public IActionResult Browse(int id)
        {
            // Get products in selected category
            var products = _context.Products.Where(p => p.CategoryId == id).OrderBy(products => products.Name).ToList();
            
            // Load the browse page and pass it the list of products to display
            return View(products);

        }
    }
}
