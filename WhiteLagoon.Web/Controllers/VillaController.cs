using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility.Interface;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Web.Controllers
{
    //Note: The Authorize attribute is used to specify that the user need to login first
    [Authorize]
    public class VillaController : Controller
    {
        private readonly IVillaService _villaService;

        public VillaController(IVillaService villaService)
        {
            _villaService = villaService;
        }

        public IActionResult Index()
        {
            var villas = _villaService.GetAllVillas();
            return View(villas);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost] //Atrribute to identifies an action to support HTTP Post method
        public IActionResult Create(Villa obj)
        {
            //Custom Model Validation
            if (obj.Name == obj.Description)
            {
                ModelState.AddModelError("name", "The description cannot exactly match the Name.");
            }
            if (ModelState.IsValid)
            {
                _villaService.CreateVilla(obj);
                TempData["success"] = "The villa has been created successfully.";
                return RedirectToAction(nameof(Index)); //Redirect to the Index action in the same controller
                                                        //return RedirectToAction("Index", "Villa"); //Redirecto the Index action the Villa Controller
            }
            TempData["error"] = "The villa could not be created.";
            return View();
        }

        public IActionResult Update(int villaId)
        {
            Villa? obj = _villaService.GetVillaById(villaId);
            /*Villa? obj = _db.Villas.Find(villaId);
            var VillaList = _db.Villas.Where(u => u.Price > 50 && u.Occupancy > 0);*/
            if (obj == null)
            {
                /*return NotFound();*/
                return RedirectToAction("Error", "Home");
            }
            return View(obj);
        }

        [HttpPost] //Atrribute to identifies an action to support HTTP Post method
        public IActionResult Update(Villa obj)
        {
            if (ModelState.IsValid && obj.Id > 0)
            {
                _villaService.UpdateVilla(obj);
                TempData["success"] = "The villa has been updated successfully.";
                return RedirectToAction(nameof(Index)); //Redirect to the Index action in the same controller
                                                        //return RedirectToAction("Index", "Villa"); //Redirecto the Index action the Villa Controller
            }
            TempData["error"] = "The villa could not be updated.";
            return View();
        }

        public IActionResult Delete(int villaId)
        {
            Villa? obj = _villaService.GetVillaById(villaId);
            /*Villa? obj = _db.Villas.Find(villaId);
            var VillaList = _db.Villas.Where(u => u.Price > 50 && u.Occupancy > 0);*/
            if (obj == null)
            {
                /*return NotFound();*/
                return RedirectToAction("Error", "Home");
            }
            return View(obj);
        }

        [HttpPost] //Atrribute to identifies an action to support HTTP Post method
        public IActionResult Delete(Villa obj)
        {
            bool deleted = _villaService.DeleteVilla(obj.Id);
            if (deleted)
            {
                TempData["success"] = "The villa has been deleted successfully.";
                return RedirectToAction("Index"); //Redirect to the Index action in the same controller
                                                  //return RedirectToAction("Index", "Villa"); //Redirecto the Index action the Villa Controller
            }
            TempData["error"] = "Failed to delete the villa.";
            return View();
        }
    }
}
