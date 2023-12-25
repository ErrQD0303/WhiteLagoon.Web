﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility.Interface;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Application.Common.Utility.Implementation
{
    public class DashBoardService : IDashBoardService
    {
        private readonly IUnitOfWork _unitOfWork;
        static int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
        readonly DateTime previousMonthStartDate = new(DateTime.Now.Year, previousMonth, 1);
        readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DashBoardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<PieChartDto> GetBookingPieChartData()
        {
            var totalBookings = _unitOfWork.Booking
                .GetAll(u => u.BookingDate >= DateTime.Now.AddDays(-30) && (u.Status != SD.StatusPending || u.Status == SD.StatusCancelled));

            var customerWithOneBooking = totalBookings.GroupBy(b => b.UserId).Where(x => x.Count() == 1).Select(x => x.Key).ToList();

            int bookingsByNewCustomer = customerWithOneBooking.Count();
            int bookingByReturningCustomer = totalBookings.Count() - bookingsByNewCustomer;

            PieChartDto pieChartVM = new()
            {
                Labels = new string[] { "New Customers Bookings", "Returning Customer Bookings" },
                Series = new decimal[] { bookingsByNewCustomer, bookingByReturningCustomer }
            };

            return pieChartVM;
        }

        public async Task<LineChartDto> GetMemberAndBookingLineChartData()
        {

            var bookingData = _unitOfWork.Booking.GetAll(u => u.BookingDate >= DateTime.Now.AddDays(-30) && u.BookingDate <= DateTime.Now)
                .GroupBy(b => b.BookingDate.Date)
                .Select(u => new
                {
                    DateTime = u.Key,
                    NewBookingCount = u.Count()
                });

            var customerData = _unitOfWork.User.GetAll(u => u.CreatedAt >= DateTime.Now.AddDays(-30) && u.CreatedAt <= DateTime.Now)
                .GroupBy(b => b.CreatedAt.Date)
                .Select(u => new
                {
                    DateTime = u.Key,
                    NewCustomerCount = u.Count()
                });

            //left join
            var leftJoin = bookingData.GroupJoin(customerData, booking => booking.DateTime, customer => customer.DateTime, (booking, customers) => new
            {
                booking.DateTime,
                booking.NewBookingCount,
                NewCustomerCount = customers.Select(x => x.NewCustomerCount).FirstOrDefault()
            });

            //right join
            var rightJoin = customerData.GroupJoin(bookingData, customer => customer.DateTime, booking => booking.DateTime, (customer, bookings) => new
            {
                customer.DateTime,
                NewBookingCount = bookings.Select(x => x.NewBookingCount).FirstOrDefault(),
                customer.NewCustomerCount
            });

            //union into full outer join
            var mergedData = leftJoin.Union(rightJoin);

            var newBookingData = mergedData.Select(x => x.NewBookingCount).ToArray();
            var newCustomerData = mergedData.Select(x => x.NewCustomerCount).ToArray();
            var categories = mergedData.Select(x => x.DateTime.ToString("MM/dd/yyyy")).ToArray();

            List<ChartData> chartDataList = new()
            {
                new()
                {
                    Name = "New Bookings",
                    Data = newBookingData
                },
                new()
                {
                    Name = "New Members",
                    Data = newCustomerData
                }
            };

            LineChartDto lineChartVM = new()
            {
                Categories = categories,
                Series = chartDataList
            };

            return lineChartVM;
        }

        public async Task<RadialBarChartDto> GetRegisteredUserChartData()
        {
            var totalUsers = _unitOfWork.User.GetAll();

            var countByCurrentMonth = totalUsers.Count(u =>
                u.CreatedAt >= currentMonthStartDate && u.CreatedAt <= DateTime.Now);
            var countByPreviousMonth = totalUsers.Count(u =>
                u.CreatedAt >= previousMonthStartDate && u.CreatedAt < currentMonthStartDate);
            return SD.GetRadialChartDataModel(totalUsers.Count(), countByCurrentMonth, countByPreviousMonth);
        }

        public async Task<RadialBarChartDto> GetRevenueChartData()
        {
            var totalBookings = _unitOfWork.Booking
                .GetAll(u => u.Status != SD.StatusPending || u.Status == SD.StatusCancelled);

            var totalRevenue = Convert.ToInt32(totalBookings.Sum(u => u.TotalCost));

            var countByCurrentMonth = totalBookings.Where(u =>
                u.BookingDate >= currentMonthStartDate && u.BookingDate <= DateTime.Now).Sum(u => u.TotalCost);
            var countByPreviousMonth = totalBookings.Where(u =>
                u.BookingDate >= previousMonthStartDate && u.BookingDate < currentMonthStartDate).Sum(u => u.TotalCost);

            return SD.GetRadialChartDataModel(totalRevenue, countByCurrentMonth, countByPreviousMonth);

        }

        public async Task<RadialBarChartDto> GetTotalBookingRadialChartData()
        {
            var totalBookings = _unitOfWork.Booking
                            .GetAll(u => u.Status != SD.StatusPending || u.Status == SD.StatusCancelled);

            var countByCurrentMonth = totalBookings.Count(u =>
                u.BookingDate >= currentMonthStartDate && u.BookingDate <= DateTime.Now);
            var countByPreviousMonth = totalBookings.Count(u =>
                u.BookingDate >= previousMonthStartDate && u.BookingDate < currentMonthStartDate);

            return SD.GetRadialChartDataModel(totalBookings.Count(), countByCurrentMonth, countByPreviousMonth);
        }
    }
}