﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GraniteHouse.Data;
using GraniteHouse.Models;
using GraniteHouse.Models.ViewModel;
using GraniteHouse.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraniteHouse.Areas.Admin.Controllers
{
    [Authorize(Roles = StaticDetails.SuperAdminEndUser + "," + StaticDetails.AdminEndUser)]
    [Area("Admin")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private int PageSize = 2;

        public AppointmentsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string searchName = null, string searchEmail = null, string searchPhone = null, string searchAppointmentDate = null, int productPage=1)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            AppointmentsViewModel appointmentVM = new AppointmentsViewModel()
            {
                Appointments = new List<Models.Appointments>()
            };

            StringBuilder param = new StringBuilder();
            param.Append("/Admin/Appointments?productPage=:");
            param.Append("&searchName=");
            if (searchName != null)
            {
                param.Append(searchName);
            }

            param.Append("&searchEmail=");
            if (searchEmail != null)
            {
                param.Append(searchEmail);
            }

            param.Append("&searchPhone=");
            if (searchPhone != null)
            {
                param.Append(searchPhone);
            }

            param.Append("&searchAppointmentDate=");
            if (searchAppointmentDate != null)
            {
                param.Append(searchAppointmentDate);
            }

            appointmentVM.Appointments = _db.Appointments.Include(a => a.SalesPerson).ToList();

            if (User.IsInRole(StaticDetails.AdminEndUser))
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.SalesPersonId == claim.Value).ToList();
            }

            if (searchName != null)
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.CustomerName.ToLower().Contains(searchName.ToLower())).ToList();
            }

            if (searchEmail != null)
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.CustomerEmail.ToLower().Contains(searchEmail.ToLower())).ToList();
            }

            if (searchPhone != null)
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.CustomerPhone.ToLower().Contains(searchPhone.ToLower())).ToList();
            }

            if (searchAppointmentDate != null)
            {
                try
                {
                    DateTime appointmentDate = Convert.ToDateTime(searchAppointmentDate);
                    appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.AppointmentDate.ToShortDateString().Equals(appointmentDate.ToShortDateString())).ToList();
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }

            var count = appointmentVM.Appointments.Count;
            appointmentVM.Appointments = appointmentVM.Appointments.OrderBy(p => p.AppointmentDate).Skip((productPage - 1) * PageSize).Take(PageSize).ToList();
            appointmentVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItems = count,
                UrlParam = param.ToString()
            };

            return View(appointmentVM);
        }

        // Get Edit Appointments
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productsList = (IEnumerable<Products>)(from product in _db.Products
                                                   join appointment in _db.ProductsSelectedForAppointment
                                                   on product.Id equals appointment.ProductId
                                                   where appointment.AppointmentId == id
                                                   select product).Include("ProductTypes");

            AppointmentDetailsViewModel appointmentVM = new AppointmentDetailsViewModel()
            {
                Appointment = _db.Appointments.Include(a => a.SalesPerson).Where(a => a.Id == id).FirstOrDefault(),
                SalesPerson = _db.ApplicationUsers.ToList(),
                Products = productsList.ToList()
            };

            return View(appointmentVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentDetailsViewModel appointmentVM)
        {
            if(ModelState.IsValid)
            {
                appointmentVM.Appointment.AppointmentDate = appointmentVM.Appointment.AppointmentDate
                    .AddHours(appointmentVM.Appointment.AppointmentTime.Hour)
                    .AddMinutes(appointmentVM.Appointment.AppointmentTime.Minute);

                var appointmentFromDb = _db.Appointments.Where(a => a.Id == appointmentVM.Appointment.Id).FirstOrDefault();

                appointmentFromDb.CustomerName = appointmentVM.Appointment.CustomerName;
                appointmentFromDb.CustomerEmail = appointmentVM.Appointment.CustomerEmail;
                appointmentFromDb.CustomerPhone = appointmentVM.Appointment.CustomerPhone;
                appointmentFromDb.AppointmentDate = appointmentVM.Appointment.AppointmentDate;
                appointmentFromDb.IsConfirmed = appointmentVM.Appointment.IsConfirmed;

                if(User.IsInRole(StaticDetails.SuperAdminEndUser)) {
                    appointmentFromDb.SalesPersonId = appointmentVM.Appointment.SalesPersonId;
                }

                _db.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(appointmentVM);
        }

        // Get Details Appointments
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productsList = (IEnumerable<Products>)(from product in _db.Products
                                                       join appointment in _db.ProductsSelectedForAppointment
                                                       on product.Id equals appointment.ProductId
                                                       where appointment.AppointmentId == id
                                                       select product).Include("ProductTypes");

            AppointmentDetailsViewModel appointmentVM = new AppointmentDetailsViewModel()
            {
                Appointment = _db.Appointments.Include(a => a.SalesPerson).Where(a => a.Id == id).FirstOrDefault(),
                SalesPerson = _db.ApplicationUsers.ToList(),
                Products = productsList.ToList()
            };

            return View(appointmentVM);
        }

        // Get Delete Appointment
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productsList = (IEnumerable<Products>)(from product in _db.Products
                                                       join appointment in _db.ProductsSelectedForAppointment
                                                       on product.Id equals appointment.ProductId
                                                       where appointment.AppointmentId == id
                                                       select product).Include("ProductTypes");

            AppointmentDetailsViewModel appointmentVM = new AppointmentDetailsViewModel()
            {
                Appointment = _db.Appointments.Include(a => a.SalesPerson).Where(a => a.Id == id).FirstOrDefault(),
                SalesPerson = _db.ApplicationUsers.ToList(),
                Products = productsList.ToList()
            };

            return View(appointmentVM);
        }

        // POST Delete Appointment
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            _db.Appointments.Remove(appointment);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}