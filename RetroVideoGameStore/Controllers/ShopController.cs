using Microsoft.AspNetCore.Mvc;

namespace RetroVideoGameStore.Controllers
{
    public class ShopController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
