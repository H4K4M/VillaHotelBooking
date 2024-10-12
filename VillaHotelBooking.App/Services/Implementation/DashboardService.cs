using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillaHotelBooking.App.Common.Interfaces;
using VillaHotelBooking.App.Common.Utility;
using VillaHotelBooking.App.Services.Interface;
using VillaHotelBooking.Web.ViewModels;

namespace VillaHotelBooking.App.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        static int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
        readonly DateTime previousMonthStartDate = new(DateTime.Now.Year, previousMonth, 1);
        readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

        private readonly IUnitOfWork _unitOfWork;
        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PieChartDto> GetBookinPieChartData()
        {
            var totalBookings = _unitOfWork.Bookings.GetAll(
                u => u.BookingDate >= DateTime.Now.AddDays(-30)
                && (u.Status != SD.StatusPending || u.Status == SD.StatusCancelled));

            var customerWithOneBooking = totalBookings.GroupBy(b => b.UserId).Where(x => x.Count() == 1).
                Select(x => x.Key).ToList();

            int bookingsByNewCustomer = customerWithOneBooking.Count();
            int bookingsByReturningCustomer = totalBookings.Count() - bookingsByNewCustomer;

            PieChartDto pieChartDto = new()
            {
                Series = new decimal[] { bookingsByNewCustomer, bookingsByReturningCustomer },
                Labels = new string[] { "New Customer Bookings", "Returning Customer Bookings" }
            };

            return pieChartDto;
        }

        public async Task<LineChartDto> GetMemberAndBookingLineChartData()
        {
            var bookingData = _unitOfWork.Bookings.GetAll(u => u.BookingDate >= DateTime.Now.AddDays(-30)
            && u.BookingDate.Date <= DateTime.Now)
                .GroupBy(b => b.BookingDate.Date)
                .Select(u => new
                {
                    DateTime = u.Key,
                    NewBookingCount = u.Count(),
                });

            var customerData = _unitOfWork.ApplicationUsers.GetAll(u => u.CreatedAt >= DateTime.Now.AddDays(-30)
            && u.CreatedAt.Date <= DateTime.Now)
                .GroupBy(b => b.CreatedAt.Date)
                .Select(u => new
                {
                    DateTime = u.Key,
                    NewCustomerCount = u.Count(),
                });
            var leftJoin = bookingData.GroupJoin(customerData, b => b.DateTime, c => c.DateTime,
                               (b, c) => new
                               {
                                   b.DateTime,
                                   b.NewBookingCount,
                                   NewCustomerCount = c.Select(x => x.NewCustomerCount).FirstOrDefault()
                               });
            var rightJoin = customerData.GroupJoin(bookingData, c => c.DateTime, b => b.DateTime,
                               (c, b) => new
                               {
                                   c.DateTime,
                                   NewBookingCount = b.Select(x => x.NewBookingCount).FirstOrDefault(),
                                   c.NewCustomerCount,

                               });

            var mergedData = leftJoin.Union(rightJoin).OrderBy(x => x.DateTime).ToList();

            var newBookingData = mergedData.Select(x => x.NewBookingCount).ToArray();
            var newCustomerData = mergedData.Select(x => x.NewCustomerCount).ToArray();

            var categories = mergedData.Select(x => x.DateTime.ToString("dd/MM/yyyy")).ToArray();


            //List<ChartData> chartDataList = new() {
            //    new ChartData {
            //        Name = "New Bookings",
            //        Data = newBookingData
            //    },
            //    new ChartData
            //    {
            //        Name = "New Customers",
            //        Data = newCustomerData
            //    }
            //};
            LineChartDto LineChartDto = new()
            {
                Series = new List<ChartData>
                {
                    new ChartData { Name = "New Bookings", Data = newBookingData },
                    new ChartData { Name = "New Members", Data = newCustomerData }

                },
                Categories = categories
            };

            return LineChartDto;
        }

        public async Task<RadialBarChartDto> GetRegisteredUserRadialChartData()
        {
            var totalUsers = _unitOfWork.ApplicationUsers.GetAll();

            var countByCurrentMonth = totalUsers.Count(u => u.CreatedAt >= currentMonthStartDate
            && u.CreatedAt <= DateTime.Now);

            var countByPreviousMonth = totalUsers.Count(u => u.CreatedAt >= previousMonthStartDate
            && u.CreatedAt <= currentMonthStartDate);



            return SD.GetRadialChartDataModel(totalUsers.Count(), countByCurrentMonth, countByPreviousMonth);
        }

        public async Task<RadialBarChartDto> GetRevenueRadialChartData()
        {
            var totalBookings = _unitOfWork.Bookings.GetAll(u => u.Status != SD.StatusPending
            || u.Status == SD.StatusCancelled);

            var totalRevenue = Convert.ToInt32(totalBookings.Sum(u => u.TotalCost));

            var countByCurrentMonth = totalBookings.Where(u => u.BookingDate >= currentMonthStartDate
            && u.BookingDate <= DateTime.Now).Sum(u => u.TotalCost);

            var countByPreviousMonth = totalBookings.Where(u => u.BookingDate >= previousMonthStartDate
            && u.BookingDate <= currentMonthStartDate).Sum(u => u.TotalCost);

            return SD.GetRadialChartDataModel(totalRevenue, countByCurrentMonth, countByPreviousMonth);
        }

        public async Task<RadialBarChartDto> GetTotalBookingRadialChartData()
        {
            var totalBookings = _unitOfWork.Bookings.GetAll(u => u.Status != SD.StatusPending
            || u.Status == SD.StatusCancelled);

            var countByCurrentMonth = totalBookings.Count(u => u.BookingDate >= currentMonthStartDate
            && u.BookingDate <= DateTime.Now);

            var countByPreviousMonth = totalBookings.Count(u => u.BookingDate >= previousMonthStartDate
            && u.BookingDate <= currentMonthStartDate);

            return SD.GetRadialChartDataModel(totalBookings.Count(), countByCurrentMonth, countByPreviousMonth);
        }
        

    }
}
