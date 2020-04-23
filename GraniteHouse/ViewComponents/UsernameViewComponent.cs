using GraniteHouse.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GraniteHouse.ViewComponents
{
    public class UsernameViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public UsernameViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var _ClaimsIdentity = (ClaimsIdentity)this.User.Identity;
            var Claims = _ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var UserFromDb = await _db.ApplicationUsers.Where(user => user.Id == Claims.Value).FirstOrDefaultAsync();

            return View(UserFromDb);
        }
    }
}
