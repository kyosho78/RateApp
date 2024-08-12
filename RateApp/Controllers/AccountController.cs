using RateApp.Models;
using RateApp.ViewModels;
using System;
using System.Linq;
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

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var hashedPassword = HashPassword(model.PasswordHash); // Hash the entered password

                // Check if the user exists in the Users table
                var user = db_context.Users.FirstOrDefault(u => u.Email == model.Email && u.PasswordHash == hashedPassword);
                if (user != null)
                {
                    // Set session for user
                    Session["UserId"] = user.UserId.ToString();
                    Session["UserName"] = user.UserName.ToString();

                    return RedirectToAction("Index", "Home");
                }

                // Check if the supplier exists in the Suppliers table
                var supplier = db_context.Suppliers.FirstOrDefault(s => s.Email == model.Email && s.PasswordHash == hashedPassword);
                if (supplier != null)
                {
                    // Set session for supplier
                    Session["SupplierId"] = supplier.SupplierId.ToString();
                    Session["SupplierName"] = supplier.SupplierName.ToString();

                    return RedirectToAction("Index", "Home");
                }

                // If no user or supplier found, show error
                ModelState.AddModelError("", "Invalid email or password.");
            }

            return View(model);
        }



        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
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
