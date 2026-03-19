using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            // Identify the customer
            var customerId = GetCustomerId();

            // Check to see if product is already in the cart
            var cartItem = _context.Carts.SingleOrDefault(c => c.ProductId == ProductId && c.CustomerId == customerId);

            if (cartItem != null)
            {
                // Product already exists, so update quantity instead
                cartItem.Quantity += Quantity;
                _context.Update(cartItem);
                _context.SaveChanges();
            }
            else
            {
                // Create a new Cart object
                var cart = new Cart
                {
                    ProductId = ProductId,
                    Quantity = Quantity,
                    Price = price,
                    CustomerId = customerId,
                    DateCreated = DateTime.Now
                };
                // Use the Carts DbSet in ApplicationContext.cs to save to the dB
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }
            
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

        // Shop/Cart
        public IActionResult Cart()
        {
            // Get CustomerId from the session variable
            var customerId = HttpContext.Session.GetString("CustomerId");
            // Get items in the customer's cart
            var cartItems = _context.Carts.Include(c => c.Product).Where(c => c.CustomerId == customerId).ToList();

            // Count number of items in cart and write session variable to display in navbar
            var itemCount = (from c in _context.Carts
                             where c.CustomerId == customerId
                             select c.Quantity).Sum();
            HttpContext.Session.SetInt32("ItemCount", itemCount);

            // Load the cart page and display the customer's items
            return View(cartItems);
        }

        // GET: /Shop/RemoveFromCart/12
        public IActionResult RemoveFromCart(int id)
        {
            // Remove the selected item from the Carts table
            var cartItem = _context.Carts.Find(id);
            if (cartItem != null)
            {
                _context.Carts.Remove(cartItem);
                _context.SaveChanges();
            }
            // Redirect to the updated Cart page
            return RedirectToAction("Cart");
        }

        // GET: /Shop/Checkout
        [Authorize]
        public IActionResult Checkout()
        {
            return View();
        }

        // POST: /Shop/Checkout
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout([Bind("FirstName,LastName,Address,City,Province,PostalCode,Phone")] Order order)
        {
            // Auto-fill the 3 properties we removed from the form
            order.OrderDate = DateTime.Now;
            order.CustomerId = User.Identity.Name;
            order.OrderTotal = (from c in _context.Carts
                                where c.CustomerId == HttpContext.Session.GetString("CustomerId")
                                select c.Quantity * c.Price).Sum();

            // Now store order in a session variable

            // Load the payment page
            return RedirectToAction("Payment");
        }
    }
}
