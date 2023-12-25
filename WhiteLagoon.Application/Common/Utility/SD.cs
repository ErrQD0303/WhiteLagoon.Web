using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Application.Common.Utility
{
    public static class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusCheckedIn = "CheckedIn";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public static int VillaRoomsAvailable_Count(int villaId, 
            List<VillaNumber> villaNumberList, DateOnly checkInDate, 
            int nights, List<Booking> bookings)
        {
            List<int> bookingInDate = new();

            var roomsInVilla = villaNumberList.Where(v => v.VillaId == villaId).Count();
            int finalAvailableRoomForAllNights = int.MaxValue;

            for (int i = 0; i < nights; ++i)
            {
                var villasBooked = bookings.Where(b
                    => b.VillaId == villaId && b.CheckInDate <= checkInDate.AddDays(i)
                    && b.CheckOutDate > checkInDate.AddDays(i));

                foreach (var booking in villasBooked)
                {
                    if (!bookingInDate.Contains(booking.Id))
                    {
                        bookingInDate.Add(booking.Id);
                    }
                }

                var totalAvailableRooms = roomsInVilla - bookingInDate.Count();
                if(totalAvailableRooms == 0)
                {
                    return 0;
                }
                else if (totalAvailableRooms < finalAvailableRoomForAllNights)
                {
                    finalAvailableRoomForAllNights = totalAvailableRooms;
                }
            }

            return finalAvailableRoomForAllNights;
        }

        public static RadialBarChartDto GetRadialChartDataModel(int totalCount, double currentMonthCount, double prevMonthCount)
        {
            RadialBarChartDto radialBarChartVM = new();

            int increaseDecreaseRatio = 100;

            if (prevMonthCount != 0)
            {
                increaseDecreaseRatio = Convert.ToInt32((currentMonthCount - prevMonthCount) / prevMonthCount * 100);
            }

            radialBarChartVM.TotalCount = totalCount;
            radialBarChartVM.CountInCurrentMonth = Convert.ToInt32(currentMonthCount);
            radialBarChartVM.HasRatioIncreased = currentMonthCount > prevMonthCount;
            radialBarChartVM.Series = new int[] { increaseDecreaseRatio };
            return radialBarChartVM;
        }

        public static void SetCulture(HttpRequest request)
        {
            string? userCulture = request.GetTypedHeaders().AcceptLanguage?.OrderByDescending(x => x.Quality ?? 1.0)
                .Select(x => x.Value).FirstOrDefault().ToString();
            if (!String.IsNullOrEmpty(userCulture))
                System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(userCulture);
            else
            {
                System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            }
        }
    }
}
