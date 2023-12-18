using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Diagnostics;
using System.Globalization;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Web.Models;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
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
            SetCulture();

            HomeVM homeVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity"),
                Nights = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.Now)
            };
            return View(homeVM);
        }

        private void SetCulture()
        {
            string? userCulture = Request.GetTypedHeaders().AcceptLanguage?.OrderByDescending(x => x.Quality ?? 1.0)
                .Select(x => x.Value).FirstOrDefault().ToString();
            if (!String.IsNullOrEmpty(userCulture))
                System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(userCulture);
            else
            {
                System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            }
        }

        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity");
            var villaNumberList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(b
                => b.Status == SD.StatusApproved || b.Status == SD.StatusCheckedIn).ToList();
            
            foreach (var villa in villaList)
            {
                int roomAvailable =
                    SD.VillaRoomsAvailable_Count(villa.Id, villaNumberList,
                    checkInDate, nights, bookedVillas);
                villa.IsAvailable = roomAvailable > 0 ? true : false;
            }

            HomeVM homeVM = new()
            {
                CheckInDate = checkInDate,
                VillaList = villaList,
                Nights = nights
            };

            return PartialView("_VillaList", homeVM); //PartialView is used to return
                                                      //a partial view
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            /*            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });*/
            return View();
        }
    }
}
