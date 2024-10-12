using Microsoft.AspNetCore.Mvc;
using System.Linq;
using VillaHotelBooking.App.Common.Interfaces;
using VillaHotelBooking.App.Common.Utility;
using VillaHotelBooking.App.Services.Interface;
using VillaHotelBooking.Web.ViewModels;

namespace VillaHotelBooking.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetTotalBookingRadialChartData()
        {
            return Json(await _dashboardService.GetTotalBookingRadialChartData());
        }

        public async Task<IActionResult> GetRegisteredUserRadialChartData()
        {
            return Json(await _dashboardService.GetRegisteredUserRadialChartData());
        }

        public async Task<IActionResult> GetRevenueRadialChartData()
        {
            return Json(await _dashboardService.GetRevenueRadialChartData());
        }

        public async Task<IActionResult> GetBookinPieChartData()
        {
            return Json(await _dashboardService.GetBookinPieChartData());
        }

        public async Task<IActionResult> GetMemberAndBookingLineChartData()
        {
            return Json(await _dashboardService.GetMemberAndBookingLineChartData());
        }

        
    }
}
