using Microsoft.AspNetCore.Mvc;

namespace WhatsAppServices.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
