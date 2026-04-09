using Microsoft.AspNetCore.Mvc;

namespace RetroVideoGameStore.Controllers
{
    public class ExampleController : Controller
    {
        public IActionResult Index()
        {
            return View("Index");
        }
    }
}
