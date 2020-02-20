using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraniteHouse.Data;
using GraniteHouse.Extensions;
using GraniteHouse.Models;
using GraniteHouse.Models.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace GraniteHouse.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public ShoppingCartViewModel ShoppingCartVM { get; set;  }

        public ShoppingCartController(ApplicationDbContext db)
        {
            _db = db;

            ShoppingCartVM = new ShoppingCartViewModel()
            {
                Products = new List<Models.Products>()
            };
        }

        // Get Index Shopping Cart
        public async Task<IActionResult> Index()
        {
            List<int> lstShoppingCart = HttpContext.Session.Get<List<int>>("sesShoppingCart");

            if(lstShoppingCart != null && lstShoppingCart.Count > 0 )
            {
                foreach(int cartItem in lstShoppingCart)
                {
                    Products prod = _db.Products.Include(p => p.SpecialTags).Include(p=>p.ProductTypes).Where(p => p.Id == cartItem).FirstOrDefault();
                    ShoppingCartVM.Products.Add(prod);
                }
            }
            return View(ShoppingCartVM);
        }

        // Post Appointment
        [HttpPost, ActionName("Index")]
        [ValidateAntiForgeryToken]
        public IActionResult IndexPost()
        {
            List<int> lstShoppingCartItems = HttpContext.Session.Get<List<int>>("sesShoppingCart");

            ShoppingCartVM.Appointments.AppointmentDate = ShoppingCartVM.Appointments.AppointmentDate
                                                            .AddHours(ShoppingCartVM.Appointments.AppointmentTime.Hour)
                                                            .AddMinutes(ShoppingCartVM.Appointments.AppointmentTime.Minute);

            Appointments appointments = ShoppingCartVM.Appointments;
            _db.Appointments.Add(appointments);
            _db.SaveChanges();

            int appointmentId = appointments.Id;

            foreach(int productId in lstShoppingCartItems)
            {
                ProductsSelectedForAppointment productsSelectedForAppointment = new ProductsSelectedForAppointment()
                {
                    AppointmentId = appointmentId,
                    ProductId = productId
                };
                _db.ProductsSelectedForAppointment.Add(productsSelectedForAppointment);
            }
            _db.SaveChanges();
            lstShoppingCartItems = new List<int>();
            HttpContext.Session.Set("sesShoppingCart", lstShoppingCartItems);

            return RedirectToAction("AppointmentConfirmation", "ShoppingCart", new { Id = appointmentId });
        }

        // Remove an item from the Shopping Cart
        public IActionResult Remove(int id)
        {
            List<int> lstShoppingCartItems = HttpContext.Session.Get<List<int>>("sesShoppingCart");

            if(lstShoppingCartItems.Count > 0)
            {
                if(lstShoppingCartItems.Contains(id))
                {
                    lstShoppingCartItems.Remove(id);
                }
            }

            HttpContext.Session.Set("sesShoppingCart", lstShoppingCartItems);

            return RedirectToAction(nameof(Index));
        }

        // Get Appointment Confirmation
        public IActionResult AppointmentConfirmation(int id)
        {
            ShoppingCartVM.Appointments = _db.Appointments.Where(a => a.Id == id).FirstOrDefault();
            List<ProductsSelectedForAppointment> objProdList = _db.ProductsSelectedForAppointment.Where(p => p.AppointmentId == id).ToList();

            foreach(ProductsSelectedForAppointment pSFAObj in objProdList)
            {
                ShoppingCartVM.Products.Add(_db.Products.Include(p => p.ProductTypes).Include(p => p.SpecialTags).Where(p => p.Id == pSFAObj.ProductId).FirstOrDefault());
            }

            return View(ShoppingCartVM);
        }
    }
}