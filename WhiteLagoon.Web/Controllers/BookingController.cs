using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Stripe;
using Stripe.Checkout;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Common.Utility.Interface;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Repository;

namespace WhiteLagoon.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IVillaService _villaService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVillaNumberService _villaNumberService;

        public BookingController(IBookingService bookingService, 
            IWebHostEnvironment webHostEnvironment, 
            IVillaService villaService, 
            UserManager<ApplicationUser> userManager, 
            IVillaNumberService villaNumberService)
        {
            _bookingService = bookingService;
            _webHostEnvironment = webHostEnvironment;
            _villaService = villaService;
            _userManager = userManager;
            _villaNumberService = villaNumberService;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            //Get the claims from User.IdentityAA
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            //Get the user id from the claims
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            //Get the user object from the database
            ApplicationUser user = _userManager.FindByIdAsync(userId).GetAwaiter().GetResult();
            checkInDate = checkInDate.AddDays(0);

            Booking booking = new()
            {
                VillaId = villaId,
                Villa = _villaService.GetVillaById(villaId),
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
            var villa = _villaService.GetVillaById(booking.VillaId);
            booking.TotalCost = villa.Price * booking.Nights;

            booking.Status = SD.StatusPending;
            booking.BookingDate = DateTime.Now;

            if (!_villaService.IsVillaAvailableByDate(villa.Id, booking.Nights, booking.CheckInDate))
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

            _bookingService.CreateBooking(booking);

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

            _bookingService.UpdateStripePaymentID(booking.Id, session.Id, session.PaymentIntentId);
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            Booking bookingFromDb = _bookingService.GetBookingById(bookingId);

            if (bookingFromDb.Status == SD.StatusPending)
            {
                var service = new SessionService();
                Session session = service.Get(bookingFromDb.StripeSessionId);

                if (session.PaymentStatus == "paid")
                {
                    _bookingService.UpdateStatus(bookingId, SD.StatusApproved, 0);
                    _bookingService.UpdateStripePaymentID(bookingFromDb.Id, session.Id, session.PaymentIntentId);
                }
            }

            return View(bookingId);
        }

        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            Booking bookingFromDb = _bookingService.GetBookingById(bookingId);

            if (bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == SD.StatusApproved)
            {
                var availableVillaNumbers = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId);
                bookingFromDb.VillaNumbers = _villaNumberService
                    .GetAllVillaNumbers()
                    .Where(v =>
                        v.VillaId == bookingFromDb.VillaId && availableVillaNumbers.Any(x => x == v.Villa_Number))
                    .ToList();
            }

            return View(bookingFromDb);
        }

        [HttpPost]
        [Authorize]
        public IActionResult GenerateInvoice(int id, string downloadType)
        {
            string basePath = _webHostEnvironment.WebRootPath;

            WordDocument document = new WordDocument();

            // Load the template.
            string dataPath = basePath + @"/exports/BookingDetails.docx";
            using FileStream fileStream = new(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            document.Open(fileStream, FormatType.Automatic);

            // Update Template
            Booking bookingFromDb = _bookingService.GetBookingById(id);

            OverrideDocumentText(document, "xx_customer_name", bookingFromDb.Name);
            OverrideDocumentText(document, "xx_customer_phone", bookingFromDb.Phone);
            OverrideDocumentText(document, "xx_customer_email", bookingFromDb.Email);
            OverrideDocumentText(document, "XX_BOOKING_NUMBER", "BOOKING ID - " + bookingFromDb.Id);
            OverrideDocumentText(document, "XX_BOOKING_DATE", "BOOKING DATE - " + bookingFromDb.BookingDate.ToShortDateString());
            OverrideDocumentText(document, "xx_payment_date", bookingFromDb.PaymentDate.ToShortDateString());
            OverrideDocumentText(document, "xx_checkin_date", bookingFromDb.CheckInDate.ToShortDateString());
            OverrideDocumentText(document, "xx_checkout_date", bookingFromDb.CheckOutDate.ToShortDateString());
            OverrideDocumentText(document, "xx_booking_total", bookingFromDb.TotalCost.ToString("c"));

            WTable table = new(document);

            table.TableFormat.Borders.LineWidth = 0.5f;
            table.TableFormat.Borders.Color = Syncfusion.Drawing.Color.Black;
            table.TableFormat.Paddings.Top = 7f;
            table.TableFormat.Paddings.Bottom = 7f;
            table.TableFormat.Borders.Horizontal.LineWidth = 1f;

            int rows = bookingFromDb.VillaNumber > 0 ? 3 : 2;
            table.ResetCells(rows, 4);
            
            AddingRowToTable(table, 0, 
                ["NIGHTS", "VILLA", "PRICE PER NIGHT", "TOTAL"]);
            
            AddingRowToTable(table, 1, 
                [bookingFromDb.Nights.ToString(),
                    bookingFromDb.Villa.Name, 
                    (bookingFromDb.TotalCost / bookingFromDb.Nights).ToString("c"), 
                    bookingFromDb.TotalCost.ToString("c")]);

            if (bookingFromDb.VillaNumber > 0)
            {
                WTableRow row2 = table.Rows[2];

                row2.Cells[0].Width = 80;
                row2.Cells[1].AddParagraph().AppendText("Villa Number - " + bookingFromDb.VillaNumber.ToString());
                row2.Cells[1].Width = 220;
                row2.Cells[3].Width = 80;
            }

            WTableStyle tableStyle = document.AddTableStyle("CustomDatStyle");
            tableStyle.TableProperties.RowStripe = 1;
            tableStyle.TableProperties.ColumnStripe = 2;
            tableStyle.TableProperties.Paddings.Top = 2;
            tableStyle.TableProperties.Paddings.Bottom = 1;
            tableStyle.TableProperties.Paddings.Left = 5.4f;
            tableStyle.TableProperties.Paddings.Right = 5.4f;

            ConditionalFormattingStyle firstRowStyle = tableStyle.ConditionalFormattingStyles.Add(ConditionalFormattingType.FirstRow);
            firstRowStyle.CharacterFormat.Bold = true;
            firstRowStyle.CharacterFormat.TextColor = Syncfusion.Drawing.Color.FromArgb(255, 255, 255, 255);
            firstRowStyle.CellProperties.BackColor = Syncfusion.Drawing.Color.Black;

            table.ApplyStyle("CustomDatStyle");

            TextBodyPart bodyPart = new(document);
            bodyPart.BodyItems.Add(table);

            document.Replace("<ADDTABLEHERE>", bodyPart, false, false);

            using DocIORenderer renderer = new();

                MemoryStream stream = new();
            if (downloadType.ToLower() == "word")
            {
                document.Save(stream, FormatType.Docx);
                stream.Position = 0;

                return File(stream, "application/docx", "BookingDetails.docx");
            }
            else
            {
                PdfDocument pdfDocument = renderer.ConvertToPDF(document);
                pdfDocument.Save(stream);
                stream.Position = 0;

                return File(stream, "application/pdf", "BookingDetails.pdf");
            }

            static void AddingRowToTable(WTable table, int rowIdx, string[] values)
            {
                WTableRow row0 = table.Rows[rowIdx];
                int i = 0;

                row0.Cells[i].AddParagraph().AppendText(values[i]);
                row0.Cells[i].Width = 80;
                ++i;
                row0.Cells[i].AddParagraph().AppendText(values[i]);
                row0.Cells[i].Width = 220;
                ++i;
                row0.Cells[i].AddParagraph().AppendText(values[i]);
                ++i;
                row0.Cells[i].AddParagraph().AppendText(values[i]);
                row0.Cells[i].Width = 80;
            }

            static void OverrideDocumentText(WordDocument document, string replaceTextValue, string propertyValue, bool caseSensitive = false, bool wholeWord = true)
            {
                TextSelection textSelection = document.Find(replaceTextValue, caseSensitive, wholeWord);
                WTextRange textRange = textSelection.GetAsOneRange();
                textRange.Text = propertyValue;
            }
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public IActionResult CheckIn(Booking booking)
        {
            _bookingService.UpdateStatus(booking.Id, SD.StatusCheckedIn, booking.VillaNumber);
            TempData["Success"] = "Booking Updated Successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public IActionResult CheckOut(Booking booking)
        {
            _bookingService.UpdateStatus(booking.Id, SD.StatusCompleted, booking.VillaNumber);
            TempData["Success"] = "Booking Completed Successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public IActionResult CancelBooking(Booking booking)
        {
            _bookingService.UpdateStatus(booking.Id, SD.StatusCancelled, booking.VillaNumber);
            TempData["Success"] = "Booking Cancelled Successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        private List<int> AssignAvailableVillaNumberByVilla(int villaId)
        {
            List<int> availableVillaNumbers = new();

            var villaNumbers = _villaNumberService
                .GetAllVillaNumbers()
                .Where(u => u.VillaId == villaId);
            var checkedInVilla = _bookingService.GetCheckedInVillaNumber(villaId);

            foreach (var villaNumber in villaNumbers)
            {
                if (!checkedInVilla.Contains(villaNumber.Villa_Number))
                {
                    availableVillaNumbers.Add(villaNumber.Villa_Number);
                }
            }

            return availableVillaNumbers;
        }

        #region API Calls
        [HttpGet]
        [Authorize]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Booking> objBookings;
            string userId = string.Empty;
            if (string.IsNullOrEmpty(status))
            {
                status = string.Empty;
            }

            if (User.IsInRole(SD.Role_Admin))
            {
                var claimsIdentity = User.Identity as ClaimsIdentity;
                userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }

            objBookings = _bookingService.GetAllBookings(userId, status);

            return Json(new { data = objBookings });
        }
        #endregion
    }
}
