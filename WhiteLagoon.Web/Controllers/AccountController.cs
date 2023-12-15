using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public IActionResult Login(string returnURL = null)
        {
            returnURL ??= Url.Content("~/");

            LoginVM loginVM = new()
            {
                RedirectURL = returnURL
            };

            return View(loginVM);
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            //Check if the model is valid
            if (ModelState.IsValid) {
                //Check if the user exists in the database
                var result = await _signInManager
                    .PasswordSignInAsync(loginVM.Email, loginVM.Password, loginVM.RememberMe, lockoutOnFailure: false);
                
                //Check if the login was successful
                if (result.Succeeded)
                {
                    //Check if the RedirectURL property is null or empty
                    if (string.IsNullOrEmpty(loginVM.RedirectURL))
                    {
                        //Redirect to the home page
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        //Redirect to the page that is stored inside RedirectURL property
                        return LocalRedirect(loginVM.RedirectURL);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                }
            }
            
            //If the model is not valid, return the view with the model
            return View(loginVM);
        }

        public async Task<IActionResult> Logout()
        {
            //Sign out the user using the SignInManager SignOutAsync method
            await _signInManager.SignOutAsync();
            //Redirect to the home page
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Register(string returnURL = null)
        {
            //Check if the returnURL is null or empty
            //If it is null or empty, assign the home page URL to it
            returnURL ??= Url.Content("~/");
            //Check if the roles exist and create them if not
            if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).Wait();
                /*await _roleManager.CreateAsync(new IdentityRole("Admin"));
                //this is equivalent to above*/
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).Wait();
            }

            RegisterVM registerVM = new()
            {
                RoleList = _roleManager.Roles.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name
                }), 
                RedirectURL = returnURL
            };

            return View(registerVM);
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (ModelState.IsValid)
            {
                /*return Redirect("https://www.facebook.com");*/
                ApplicationUser user = new()
                {
                    Name = registerVM.Name,
                    Email = registerVM.Email,
                    PhoneNumber = registerVM.PhoneNumber,
                    NormalizedEmail = registerVM.Email.ToUpper(),
                    EmailConfirmed = true,
                    UserName = registerVM.Email,
                    CreatedAt = DateTime.Now
                };

                //Create the user on the database
                var result = _userManager.CreateAsync(user, registerVM.Password).GetAwaiter().GetResult();

                //Check if the user was created successfully
                if (result.Succeeded)
                {
                    //Check if the role is selected
                    if (!string.IsNullOrEmpty(registerVM.Role))
                    {
                        //If the role is selected, assign the selected role
                        await _userManager.AddToRoleAsync(user, registerVM.Role);
                        /*_userManager.AddToRoleAsync(user, registerVM.Role).Wait();*/
                    }
                    else
                    {
                        //If the role is not selected, assign the customer role by default
                        await _userManager.AddToRoleAsync(user, SD.Role_Customer);
                    }

                    //Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    if (string.IsNullOrEmpty(registerVM.RedirectURL))
                    {
                        //Redirect to the home page
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        //Redirect to the page that is stored inside RedirectURL property
                        return LocalRedirect(registerVM.RedirectURL);
                    }
                }

                foreach (var error in result.Errors)
                {
                    //Add the error to the ModelState with the key ""
                    ModelState.AddModelError("", error.Description);
                }
            }

            //Populate the RoleList property of the RegisterVM object
            registerVM.RoleList = _roleManager.Roles.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Name
            });

            return View(registerVM);
        }
    }
}
