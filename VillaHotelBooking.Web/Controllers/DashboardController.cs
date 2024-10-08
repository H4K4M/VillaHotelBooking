﻿using Microsoft.AspNetCore.Mvc;
using System.Linq;
using VillaHotelBooking.App.Common.Interfaces;
using VillaHotelBooking.App.Common.Utility;
using VillaHotelBooking.Web.ViewModels;

namespace VillaHotelBooking.Web.Controllers
{
    public class DashboardController : Controller
    {
        static int previousMonth = DateTime.Now.Month == 1? 12 : DateTime.Now.Month - 1;
        readonly DateTime previousMonthStartDate = new(DateTime.Now.Year, previousMonth, 1);
        readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

        private readonly IUnitOfWork _unitOfWork;
        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetTotalBookingRadialChartData()
        {
            var totalBookings = _unitOfWork.Bookings.GetAll(u => u.Status != SD.StatusPending 
            || u.Status == SD.StatusCancelled);

            var countByCurrentMonth = totalBookings.Count(u => u.BookingDate >= currentMonthStartDate
            && u.BookingDate <= DateTime.Now);

            var countByPreviousMonth = totalBookings.Count(u => u.BookingDate >= previousMonthStartDate
            && u.BookingDate <= currentMonthStartDate);

            return Json(GetRadialChartDataModel(totalBookings.Count(), countByCurrentMonth, countByPreviousMonth));
        }

        public async Task<IActionResult> GetRegisteredUserRadialChartData()
        {
            var totalUsers = _unitOfWork.ApplicationUsers.GetAll();

            var countByCurrentMonth = totalUsers.Count(u => u.CreatedAt >= currentMonthStartDate
            && u.CreatedAt <= DateTime.Now);

            var countByPreviousMonth = totalUsers.Count(u => u.CreatedAt >= previousMonthStartDate
            && u.CreatedAt <= currentMonthStartDate);

            

            return Json(GetRadialChartDataModel(totalUsers.Count(), countByCurrentMonth, countByPreviousMonth));
        }

        public async Task<IActionResult> GetRevenueRadialChartData()
        {
            var totalBookings = _unitOfWork.Bookings.GetAll(u => u.Status != SD.StatusPending
            || u.Status == SD.StatusCancelled);

            var totalRevenue = Convert.ToInt32(totalBookings.Sum(u => u.TotalCost));

            var countByCurrentMonth = totalBookings.Where(u => u.BookingDate >= currentMonthStartDate
            && u.BookingDate <= DateTime.Now).Sum(u => u.TotalCost);

            var countByPreviousMonth = totalBookings.Where(u => u.BookingDate >= previousMonthStartDate
            && u.BookingDate <= currentMonthStartDate).Sum(u => u.TotalCost);



            return Json(GetRadialChartDataModel(totalRevenue, countByCurrentMonth, countByPreviousMonth));
        }

        public async Task<IActionResult> GetBookinPieChartData()
        {
            var totalBookings = _unitOfWork.Bookings.GetAll(
                u => u.BookingDate >= DateTime.Now.AddDays(-30) 
                && (u.Status != SD.StatusPending || u.Status == SD.StatusCancelled));

            var customerWithOneBooking = totalBookings.GroupBy(b => b.UserId).Where(x => x.Count() == 1).
                Select(x => x.Key).ToList();

            int bookingsByNewCustomer = customerWithOneBooking.Count();
            int bookingsByReturningCustomer = totalBookings.Count() - bookingsByNewCustomer;

            PieChartVM pieChartVM = new()
            {
                Series = new decimal[] { bookingsByNewCustomer, bookingsByReturningCustomer },
                Labels = new string[] { "New Customer Bookings", "Returning Customer Bookings" }
            };

            

            return Json(pieChartVM);
        }

        public async Task<IActionResult> GetMemberAndBookingLineChartData()
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
            LineChartVM lineChartVM = new()
            {
                Series = new List<ChartData>
                {
                    new ChartData { Name = "New Bookings", Data = newBookingData },
                    new ChartData { Name = "New Members", Data = newCustomerData }

                },
                Categories = categories
            };

            

            return Json(lineChartVM);
        }

        private static RadialBarChartVM GetRadialChartDataModel(int totalCount, double currentMonthCount, double prevMonthCount)
        {
            RadialBarChartVM radialBarChartVM = new();

            int increaseDecreaseRatio = 100;

            if (prevMonthCount != 0)
            {
                increaseDecreaseRatio = Convert.ToInt32(((currentMonthCount - prevMonthCount) / prevMonthCount) * 100);
            }

            radialBarChartVM.TotalCount = totalCount;
            radialBarChartVM.CountInCurrentMonth = Convert.ToInt32(currentMonthCount);
            radialBarChartVM.hasRatioIncrease = currentMonthCount > prevMonthCount ? true : false;
            radialBarChartVM.Series = new int[] { increaseDecreaseRatio };

            return radialBarChartVM;
        }
    }
}