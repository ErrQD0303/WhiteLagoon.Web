using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public VillaController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var villas = _unitOfWork.Villa.GetAll();
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
                _unitOfWork.Villa.Add(obj); //write a script to create the Villa object in the database
                _unitOfWork.Villa.Save(); //Go into the database and create the Villa object
                TempData["success"] = "The villa has been created successfully.";
                return RedirectToAction(nameof(Index)); //Redirect to the Index action in the same controller
                                                  //return RedirectToAction("Index", "Villa"); //Redirecto the Index action the Villa Controller
            }
            TempData["error"] = "The villa could not be created.";
            return View();
        }

        public IActionResult Update(int villaId)
        {
            Villa? obj = _unitOfWork.Villa.Get(u => u.Id == villaId);
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
                _unitOfWork.Villa.Update(obj); //write a script to update the Villa object in the database
                _unitOfWork.Villa.Save(); //Go into the database and update the Villa object
                TempData["success"] = "The villa has been updated successfully.";
                return RedirectToAction(nameof(Index)); //Redirect to the Index action in the same controller
                                                  //return RedirectToAction("Index", "Villa"); //Redirecto the Index action the Villa Controller
            }
            TempData["error"] = "The villa could not be updated.";
            return View();
        }

        public IActionResult Delete(int villaId)
        {
            Villa? obj = _unitOfWork.Villa.Get(u => u.Id == villaId);
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
            Villa? objFromDb = _unitOfWork.Villa.Get(u => u.Id == obj.Id);
            if (objFromDb is not null)
            {
                _unitOfWork.Villa.Remove(objFromDb); //write a script to delete the Villa object in the database
                _unitOfWork.Villa.Save(); //Go into the database and delete the Villa object
                TempData["success"] = "The villa has been deleted successfully.";
                return RedirectToAction("Index"); //Redirect to the Index action in the same controller
                                                  //return RedirectToAction("Index", "Villa"); //Redirecto the Index action the Villa Controller
            }
            TempData["error"] = "The villa could not be deleted.";
            return View();
        }
    }
}
