using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillaHotelBooking.App.Common.Interfaces;
using VillaHotelBooking.App.Common.Utility;
using VillaHotelBooking.Domain.Entities;

namespace VillaHotelBooking.Infa.Data
{
    public class DbInitializer : IDbInitialize
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public DbInitializer(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async void Initialize()
        {
            try
            {
                if (_context.Database.GetPendingMigrations().Count() > 0)
                {
                    _context.Database.Migrate();
                }

                if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).Wait();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).Wait();

                    _userManager.CreateAsync(new ApplicationUser
                    {
                        UserName = "admin@hakam.com",
                        Email = "admin@hakam.com",
                        Name = "Admin",
                        NormalizedUserName = "ADMIN@HAKAM.COM",
                        NormalizedEmail = "ADMIN@HAKAM.COM",
                        PhoneNumber = "1234567890",
                    }, "Admin123*").GetAwaiter().GetResult();

                    ApplicationUser user = _context.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@hakam.com");

                    _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
                }

                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
