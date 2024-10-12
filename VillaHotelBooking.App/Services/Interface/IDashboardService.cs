using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillaHotelBooking.Web.ViewModels;

namespace VillaHotelBooking.App.Services.Interface
{
    public interface IDashboardService
    {
        Task<RadialBarChartDto> GetTotalBookingRadialChartData();
        Task<RadialBarChartDto> GetRegisteredUserRadialChartData();
        Task<RadialBarChartDto> GetRevenueRadialChartData();
        Task<PieChartDto> GetBookinPieChartData();
        Task<LineChartDto> GetMemberAndBookingLineChartData();

    }
}
