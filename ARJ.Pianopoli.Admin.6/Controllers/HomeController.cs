using Microsoft.AspNetCore.Mvc;

namespace ARJ.Pianopoli.Admin._6.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
