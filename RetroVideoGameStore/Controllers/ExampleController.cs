using Microsoft.AspNetCore.Mvc;

namespace RetroVideoGameStore.Controllers
{
    public class ExampleController : Controller
    {
        // Constructor method so instance of this class can be created
        public ExampleController()
        {

        }
        public IActionResult Index()
        {
            return View("Index");
        }
    }
}
