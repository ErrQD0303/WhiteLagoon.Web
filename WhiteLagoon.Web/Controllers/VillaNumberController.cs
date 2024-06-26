﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility.Interface;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly IVillaService _villaService;
        private readonly IVillaNumberService _villaNumberService;

        public VillaNumberController(IVillaService villaService, IVillaNumberService villaNumberService)
        {
            _villaService = villaService;
            _villaNumberService = villaNumberService;
        }

        public void OnGetProfile(int profileId)
        {
            ViewData["ProfileId"] = profileId;
        }

        public IActionResult Index()
        {
            var villas = _villaNumberService.GetAllVillaNumbers();
            return View(villas);
        }

        [Route("/VillaNumber/Create",
            Name = "create1")] //Route just for testing //Note from Chrome
        public IActionResult Create()
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _villaService.GetAllVillas().ToList().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };

            return View(villaNumberVM);
        }

        [Route("/VillaNumber/Create",
            Name = "create2")] //Route just for testing
        [HttpPost] //Atrribute to identifies an action to support HTTP Post method
        public IActionResult Create(VillaNumberVM obj)
        {
            /*//Solution 1
            ModelState.Remove("Villa"); //Remove the validation for the "Villa" column of VillaNumber*/
            bool roomNumberExists = _villaNumberService.CheckVillaNumberExists(obj.VillaNumber.Villa_Number);
            if (ModelState.IsValid && !roomNumberExists)
            {
                _villaNumberService.CreateVillaNumber(obj.VillaNumber);
                TempData["success"] = "The villa Number has been created successfully.";
                return RedirectToAction(nameof(Index)); //Redirect to the Index action in the same controller
                                                  //return RedirectToAction("Index", "VillaNumber"); //Redirecto the Index action the Villa Controller
            }
            TempData["error"] = "The room has already existed";
            obj.VillaList = _villaService.GetAllVillas().ToList().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            return View(obj);
        }

        public IActionResult Update(int villaNumberId)
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _villaService.GetAllVillas().ToList().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                VillaNumber = _villaNumberService.GetVillaNumberById(villaNumberId)
            };
            if (villaNumberVM.VillaNumber == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(villaNumberVM);
        }

        [HttpPost] //Atrribute to identifies an action to support HTTP Post method
        public IActionResult Update(VillaNumberVM villaNumberVM)
        {
            if (ModelState.IsValid)
            {
                _villaNumberService.UpdateVillaNumber(villaNumberVM.VillaNumber);
                TempData["success"] = "The villa has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            //populate after villaNumberVM because VillaList doesn't return
            villaNumberVM.VillaList = _villaService.GetAllVillas().ToList().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            return View(villaNumberVM);
        }

        public IActionResult Delete(int villaNumberId)
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _villaService.GetAllVillas().ToList().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                VillaNumber = _villaNumberService.GetVillaNumberById(villaNumberId)
            };
            if (villaNumberVM.VillaNumber == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(villaNumberVM);
        }

        [HttpPost] //Atrribute to identifies an action to support HTTP Post method
        public IActionResult Delete(VillaNumberVM villaNumberVM)
        {
            bool isVillaDeleted = _villaNumberService.DeleteVillaNumber(villaNumberVM.VillaNumber.Villa_Number);
            if (isVillaDeleted)
            {
                TempData["success"] = "The villa has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The villa number could not be deleted";
            return View(villaNumberVM);
        }
    }
}
