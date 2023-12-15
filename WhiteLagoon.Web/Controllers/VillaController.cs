using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Web.Controllers
{
    //Note: The Authorize attribute is used to specify that the user need to login first
    [Authorize]
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvirontment;

        public VillaController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvirontment = webHostEnvironment;
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
                if (obj.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(obj.Image.FileName);
                    string imagePath = Path.Combine(_webHostEnvirontment.WebRootPath, @"images\VillaImage");

                    using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                    obj.Image.CopyTo(fileStream);

                    obj.ImageUrl = @"\images\VillaImage\" + fileName;
                }
                else
                {
                    obj.ImageUrl = "https://placehold.co/600x400";
                }
                _unitOfWork.Villa.Add(obj); //write a script to create the Villa object in the database
                _unitOfWork.Save(); //Go into the database and create the Villa object
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
                if (obj.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(obj.Image.FileName);
                    string imagePath = Path.Combine(_webHostEnvirontment.WebRootPath, @"images\VillaImage");

                    if (!string.IsNullOrEmpty(obj.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvirontment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
                        
                        if (System.IO.File.Exists(oldImagePath)) {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                    obj.Image.CopyTo(fileStream);

                    obj.ImageUrl = @"\images\VillaImage\" + fileName;
                }

                _unitOfWork.Villa.Update(obj); //write a script to update the Villa object in the database
                _unitOfWork.Save(); //Go into the database and update the Villa object
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
                if (!string.IsNullOrEmpty(objFromDb.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvirontment.WebRootPath, objFromDb.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                _unitOfWork.Villa.Remove(objFromDb); //write a script to delete the Villa object in the database
                _unitOfWork.Save(); //Go into the database and delete the Villa object
                TempData["success"] = "The villa has been deleted successfully.";
                return RedirectToAction("Index"); //Redirect to the Index action in the same controller
                                                  //return RedirectToAction("Index", "Villa"); //Redirecto the Index action the Villa Controller
            }
            TempData["error"] = "The villa could not be deleted.";
            return View();
        }
    }
}
