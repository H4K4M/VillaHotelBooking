using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VillaHotelBooking.App.Common.Interfaces;
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

        public IActionResult Login(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");  // If returnUrl is null, set it to the root URL

            LoginVM loginVM = new LoginVM
            {
                RedirectUrl = returnUrl
            };
            return View(loginVM);
        }
        public IActionResult Register()
        {
            if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole("Admin")).Wait();
                _roleManager.CreateAsync(new IdentityRole("Customer")).Wait();
            }

            RegisterVM registerVM = new RegisterVM
            {
                RoleList = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                })
            };

            return View(registerVM);
        }
    }
}
