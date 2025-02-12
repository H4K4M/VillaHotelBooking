﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VillaHotelBooking.App.Common.Interfaces;
using VillaHotelBooking.App.Common.Utility;
using VillaHotelBooking.Domain.Entities;
using VillaHotelBooking.Web.ViewModels;

namespace VillaHotelBooking.Web.Controllers
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

        // GET: Account/Login
        public IActionResult Login(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");  // If returnUrl is null, set it to the root URL

            LoginVM loginVM = new LoginVM
            {
                RedirectUrl = returnUrl
            };
            return View(loginVM);
        }


        // POST: Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if(ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(loginVM.Email, loginVM.Password, loginVM.RememberMe, lockoutOnFailure: false);
                if(result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(loginVM.Email);
                    if (await _userManager.IsInRoleAsync(user, SD.Role_Admin))
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(loginVM.RedirectUrl) && Url.IsLocalUrl(loginVM.RedirectUrl))
                        {
                            return Redirect(loginVM.RedirectUrl);
                        }
                        return RedirectToAction("Index", "Home");
                    }
                }
                ModelState.AddModelError("", "Invalid login attempt");

                if (result.Succeeded)
                {

                    if (string.IsNullOrEmpty(loginVM.RedirectUrl))
                        return RedirectToAction("Index", "Home");
                    return LocalRedirect(loginVM.RedirectUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt");
                }
            }

            return View(loginVM);
        }

        // Get Account/Register
        public IActionResult Register(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");  // If returnUrl is null, set it to the root URL

            

            RegisterVM registerVM = new RegisterVM
            {
                RoleList = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }),
                RedirectUrl = returnUrl
            };

            return View(registerVM);
        }


        // POST: Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser
                {
                    Name = registerVM.Name,
                    Email = registerVM.Email,
                    PhoneNumber = registerVM.PhoneNumber,
                    NormalizedEmail = registerVM.Email.ToUpper(),
                    EmailConfirmed = true,
                    UserName = registerVM.Email,
                    CreatedAt = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, registerVM.Password);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(registerVM.Role))
                    {
                        await _userManager.AddToRoleAsync(user, registerVM.Role);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, SD.Role_Customer);
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    if (string.IsNullOrEmpty(registerVM.RedirectUrl))
                        return RedirectToAction("Index", "Home");
                    return LocalRedirect(registerVM.RedirectUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            registerVM.RoleList = _roleManager.Roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Name
            });

            return View(registerVM);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
