using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Stripe;
using Stripe.Checkout;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public BookingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            if (checkInDate.Equals(DateOnly.MinValue))
            {
                string? checkInDateFromRequest = Request.Query["checkInDate"];
                if (checkInDateFromRequest != null)
                    checkInDate = DateOnly.Parse(checkInDateFromRequest, System.Globalization.CultureInfo.CurrentCulture);
            }
            //Get the claims from User.IdentityAA
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            //Get the user id from the claims
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            //Get the user object from the database
            ApplicationUser user = _unitOfWork.User.Get(u => u.Id == userId);
            checkInDate = checkInDate.AddDays(0);

            Booking booking = new()
            {
                VillaId = villaId,
                Villa = _unitOfWork.Villa.Get(u => u.Id == villaId, includeProperties: "VillaAmenity"),
                CheckInDate = checkInDate,
                Nights = nights,
                CheckOutDate = checkInDate.AddDays(nights),
                UserId = userId,
                Name = user.Name,
                Email = user.Email,
                Phone = user.PhoneNumber
            };
            booking.TotalCost = booking.Villa.Price * nights;

            return View(booking);
        }

        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(Booking booking)
        {
            var villa = _unitOfWork.Villa.Get(u => u.Id == booking.VillaId);
            booking.TotalCost = villa.Price * booking.Nights;

            booking.Status = SD.StatusPending;
            booking.BookingDate = DateTime.Now;

            var villaNumberList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(b
                => b.Status == SD.StatusApproved || b.Status == SD.StatusCheckedIn).ToList();

            int roomAvailable =
                SD.VillaRoomsAvailable_Count(villa.Id, villaNumberList,
                booking.CheckInDate, booking.Nights, bookedVillas);

            if (roomAvailable == 0)
            {
                TempData["error"] = "Room has been sold out!";
                //no rooms available
                return RedirectToAction(nameof(FinalizeBooking), new
                {
                    villaId = booking.VillaId,
                    checkInDate = booking.CheckInDate,
                    nights = booking.Nights
                });
            }

            _unitOfWork.Booking.Add(booking);
            _unitOfWork.Save();

            //Get the domain name
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl = domain + $"booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}",
            };

            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(booking.TotalCost * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name,
                        //Can provide Description and Images
                        /*Images = new List<string> { villa.ImageUrl }*/
                    },
                },
                Quantity = 1
            });

            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.Booking.UpdateStripePaymentID(booking.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            Booking bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == bookingId,
                includeProperties: "User,Villa");

            if (bookingFromDb.Status == SD.StatusPending)
            {
                var service = new SessionService();
                Session session = service.Get(bookingFromDb.StripeSessionId);

                if (session.PaymentStatus == "paid")
                {
                    _unitOfWork.Booking.UpdateStatus(bookingFromDb.Id, SD.StatusApproved);
                    _unitOfWork.Booking.UpdateStripePaymentID(bookingFromDb.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Save();
                }
            }

            return View(bookingId);
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Booking> objBookings;

            if (User.IsInRole(SD.Role_Admin))
            {
                objBookings = _unitOfWork.Booking.GetAll(includeProperties: "User,Villa");
            }
            else
            {
                var claimsIdentity = User.Identity as ClaimsIdentity;
                var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                objBookings = _unitOfWork.Booking.GetAll(u => u.UserId == userId, includeProperties: "User,Villa");
            }

            if (!string.IsNullOrEmpty(status))
            {
                objBookings = objBookings.Where(u => u.Status.ToLower().Equals(status.ToLower()));
            }

            return Json(new { data = objBookings });
        }

        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            Booking bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == bookingId, includeProperties: "User,Villa");

            if (bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == SD.StatusApproved)
            {
                var availableVillaNumbers = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId);
                bookingFromDb.VillaNumbers = _unitOfWork.VillaNumber
                    .GetAll(v =>
                        v.VillaId == bookingFromDb.VillaId && availableVillaNumbers.Any(x => x == v.Villa_Number))
                    .ToList();
            }

            return View(bookingFromDb);
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public IActionResult CheckIn(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCheckedIn, booking.VillaNumber);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Updated Successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public IActionResult CheckOut(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCompleted, booking.VillaNumber);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Completed Successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public IActionResult CancelBooking(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCancelled, booking.VillaNumber);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Cancelled Successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        private List<int> AssignAvailableVillaNumberByVilla(int villaId)
        {
            List<int> availableVillaNumbers = new();

            var villaNumbers = _unitOfWork.VillaNumber.GetAll(v => v.VillaId == villaId);
            var checkedInVilla = _unitOfWork.Booking.GetAll(b => b.VillaId == villaId && b.Status == SD.StatusCheckedIn).Select(b => b.VillaNumber);

            foreach (var villaNumber in villaNumbers)
            {
                if (!checkedInVilla.Contains(villaNumber.Villa_Number))
                {
                    availableVillaNumbers.Add(villaNumber.Villa_Number);
                }
            }

            return availableVillaNumbers;
        }
    }
}
