using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VillaHotelBooking.App.Common.Interfaces;
using VillaHotelBooking.Web.Models;
using VillaHotelBooking.Web.ViewModels;

namespace VillaHotelBooking.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new()
            {
                VillaList = _unitOfWork.Villas.GetAll(includeProperties: "VillaAmenity"),
                Nights = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.Now),
            };
            return View(homeVM);
        }

        // POST: HomeController/Index
        [HttpPost]
        public IActionResult Index(HomeVM homeVM)
        {
            homeVM.VillaList = _unitOfWork.Villas.GetAll(includeProperties: "VillaAmenity");
            
            return View(homeVM);
        }

        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            var villaList = _unitOfWork.Villas.GetAll(includeProperties: "VillaAmenity").ToList();
            foreach (var villa in villaList)
            {
                if (villa.Id % 2 == 0)
                {
                    villa.IsVailable = false;
                }
            }
            HomeVM homeVM = new()
            {
                VillaList = villaList,
                Nights = nights,
                CheckInDate = checkInDate,
            };
            return PartialView("_VillaList", homeVM);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
