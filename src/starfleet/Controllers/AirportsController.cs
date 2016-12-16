using Microsoft.AspNetCore.Mvc;

namespace starfleet.Controllers
{
    public class AirportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

    }
}
