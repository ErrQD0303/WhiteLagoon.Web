using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Utility.Interface
{
    public interface IBookingService
    {
        void CreateBooking(Booking booking);
        Booking GetBookingById(int id);
        IEnumerable<Booking> GetAllBookings(string userId = "", string? statusFilterList = "");
        void UpdateStatus(int bookingId, string orderStatus, int villaNumber = 0);
        void UpdateStripePaymentID(int bookingId, string sessionId, string paymentIntentId);
        IEnumerable<int> GetCheckedInVillaNumber(int villaId);
    }
}
