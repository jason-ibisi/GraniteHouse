using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraniteHouse.Models;
using GraniteHouse.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GraniteHouse.Areas.Identity.Pages.Account
{
    public class AddAdminUserModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AddAdminUserModel (
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> OnGet()
        {
            // Create roles for website and Create SUPER ADMIN user
            if (!await _roleManager.RoleExistsAsync(StaticDetails.AdminEndUser))
            {
                await _roleManager.CreateAsync(new IdentityRole(StaticDetails.AdminEndUser));
            }

            if (!await _roleManager.RoleExistsAsync(StaticDetails.SuperAdminEndUser))
            {
                await _roleManager.CreateAsync(new IdentityRole(StaticDetails.SuperAdminEndUser));

                var userAdmin = new ApplicationUser
                {
                    UserName = "daveyjones@grr.la",
                    Email = "daveyjones@grr.la",
                    PhoneNumber = "08068107754",
                    Name = "Davey Jones"
                };

                var user = await _userManager.CreateAsync(userAdmin, "Admin123*");
                await _userManager.AddToRoleAsync(userAdmin, StaticDetails.SuperAdminEndUser);
            }

            return Page();
        }
    }
}