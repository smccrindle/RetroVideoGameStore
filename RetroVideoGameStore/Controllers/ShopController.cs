using Microsoft.AspNetCore.Mvc;
using RetroVideoGameStore.Data;
using RetroVideoGameStore.Models;

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

        // Shop/AddToCart
        [HttpPost]
        public IActionResult AddToCart(int ProductId, int Quantity)
        {
            // Get current product price
            var price = _context.Products.Find(ProductId).Price;
            // Identify the customer (they are probably anonymous)
            var customerId = GetCustomerId();
            // Create a new Cart object
            var cart = new Cart
            {
                ProductId = ProductId,
                Quantity = Quantity,
                Price = price,
                CustomerId = customerId,
                DateCreated = DateTime.Now
            };
            // Use the Carts  DbSet in ApplicationContext.cs to save to the dB
            _context.Carts.Add(cart);
            _context.SaveChanges();

            // Redirect to show the current cart
            return RedirectToAction("Cart");
        }
        private string GetCustomerId()
        {
            // Is there already a session variable holding an identifier for this customer?
            if (HttpContext.Session.GetString("CustomerId") == null)
            {
                // Cart is empty, user is unknown
                var customerId = "";
                // Use a GUID to generate a new unique identifier
                customerId = Guid.NewGuid().ToString();
                // Now store the new identifier in a session variable
                HttpContext.Session.SetString("CustomerId", customerId);

            }
            return HttpContext.Session.GetString("CustomerId");
        }
    }
}
