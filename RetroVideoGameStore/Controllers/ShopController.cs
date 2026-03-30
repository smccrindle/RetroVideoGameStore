using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http; // for HttpContext.Session.GetString/SetString
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RetroVideoGameStore.Data;
using RetroVideoGameStore.Models;

// Add references for Stripe
using Stripe;
using Stripe.Checkout;
using System.Configuration;

namespace RetroVideoGameStore.Controllers
{
    public class ShopController : Controller
    {
        // dB connection
        private readonly ApplicationDbContext _context;

        // Configuration dependency needed to read Stripe Keys from appsettings.json or the secret key store
        private IConfiguration _configuration;

        // Connect to dB whenever this controller is used
        // This controller uses Dependency Injection - it requires a db connection when it is created
        public ShopController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

            // Check to see if product already exists in the cart
            var cartItem = _context.Carts.SingleOrDefault(c => c.ProductId == ProductId && c.CustomerId == customerId);

            if (cartItem != null)
            {
                // Product already exists in cart, so update quantity instead
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
                // Use the Carts  DbSet in ApplicationContext.cs to save to the dB
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

        // GET: Shop/Cart
        public IActionResult Cart()
        {
            // Get CustomerId from the session variable
            var customerId = HttpContext.Session.GetString("CustomerId");
            // Get items in the customer's cart (and add a reference to the parent object)
            var cartItems = _context.Carts.Include(c => c.Product).Where(c => c.CustomerId == customerId).ToList();

            // Count the number of items in the cart and write a session variable to display in the navbar
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
            HttpContext.Session.SetObject("Order", order);

            // Load the payment page
            return RedirectToAction("Payment");
        }

        // GET: /Shop/Payment
        [Authorize]
        public IActionResult Payment()
        {
            // Get the order from the Session variable
            var order = HttpContext.Session.GetObject<Order>("Order");
            // Fetch and display the Order Total to the customer
            ViewBag.Total = order.OrderTotal;
            // Also use the ViewBag to set the PublishableKey, which we can read from the Configuration
            ViewBag.PublishableKey = _configuration.GetSection("Stripe")["PublishableKey"];
            // Load the Payment view
            return View();
        }

        // POST: /Shop/ProcessPayment
        // Code derived/adapted from https://docs.stripe.com/checkout/quickstart?lang=dotnet
        [HttpPost]
        [Authorize]
        public ActionResult ProcessPayment()
        {
            // Get order from session variable
            var order = HttpContext.Session.GetObject<Order>("Order");
            var orderTotal = order.OrderTotal;
            // Get Stripe Secret Key from the configuration
            StripeConfiguration.ApiKey = _configuration.GetSection("Stripe")["SecretKey"];

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                LineItems = new List<SessionLineItemOptions>
                {
                  new SessionLineItemOptions
                  {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long?)(orderTotal * 100), // amount in cents
                        Currency = "cad",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Retro Video Game Store Purchase"
                        }
                    }
                  },
                },
                Mode = "payment",
                SuccessUrl = "https://" + Request.Host + "/Shop/SaveOrder",
                CancelUrl = "https://" + Request.Host + "/Shop/Cart"
            };
            
            // Now invoke Stripe payment
            var service = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
    }
}
