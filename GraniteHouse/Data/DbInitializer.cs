using GraniteHouse.Models;
using GraniteHouse.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraniteHouse.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(ApplicationDbContext db, 
            UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async void Initialize()
        {
            if(_db.Database.GetPendingMigrations().Count() > 0)
            {
                _db.Database.Migrate();
            }

            if (_db.Roles.Any(role => role.Name == StaticDetails.SuperAdminEndUser)) return;

            _roleManager.CreateAsync(new IdentityRole(StaticDetails.AdminEndUser)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(StaticDetails.SuperAdminEndUser)).GetAwaiter().GetResult();

            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "daveyjones@grr.la",
                Email = "daveyjones@grr.la",
                Name = "Davey Jones",
                EmailConfirmed = true,
                PhoneNumber = "08058407837"
            }, "Admin123*").GetAwaiter().GetResult();

            await _userManager.AddToRoleAsync(await _userManager.FindByEmailAsync("daveyjones@grr.la"), StaticDetails.SuperAdminEndUser);
        }
    }
}
