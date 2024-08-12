using RateApp.Models;
using RateApp.ViewModels;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace RateApp.Controllers // Make sure the namespace matches your project's namespace
{
    public class AccountController : Controller
    {
        private RatingDBEntities db_context = new RatingDBEntities();

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var passwordHash = HashPassword(model.PasswordHash);

                if (model.IsSupplier)
                {
                    var supplier = new Suppliers
                    {
                        SupplierName = model.UserName,
                        Email = model.Email,
                        PasswordHash = passwordHash,
                        PhoneNumber = model.PhoneNumber,
                        Address = model.Address,
                        City = model.City,
                        Country = model.Country,
                        CreatedAt = DateTime.Now
                    };
                    db_context.Suppliers.Add(supplier);
                }
                else
                {
                    var user = new Users
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        PasswordHash = passwordHash,
                        PhoneNumber = model.PhoneNumber,
                        Address = model.Address,
                        City = model.City,
                        Country = model.Country,
                        CreatedAt = DateTime.Now
                    };
                    db_context.Users.Add(user);
                }

                db_context.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db_context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
