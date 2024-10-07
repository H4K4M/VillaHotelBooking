using Microsoft.AspNetCore.Mvc;

namespace VillaHotelBooking.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
