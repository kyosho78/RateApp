using RateApp.Models;
using RateApp.ViewModels;
using System;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Configuration;

namespace RateApp.Controllers // Make sure the namespace matches your project's namespace
{


    public class AccountController : Controller
    {
        private RatingDBEntities1 db_context = new RatingDBEntities1();


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
                    Session["UserId"] = user.UserId;
                    Session["UserName"] = user.UserName.ToString();

                    return RedirectToAction("Index", "Home");
                }

                // Check if the supplier exists in the Suppliers table
                var supplier = db_context.Suppliers.FirstOrDefault(s => s.Email == model.Email && s.PasswordHash == hashedPassword);
                if (supplier != null)
                {
                    // Set session for supplier
                    Session["SupplierId"] = supplier.SupplierId;
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




        // GET: Account/ForgotPassword
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user or supplier by email
                var user = db_context.Users.FirstOrDefault(u => u.Email == model.Email);
                var supplier = db_context.Suppliers.FirstOrDefault(s => s.Email == model.Email);

                if (user != null || supplier != null)
                {
                    // Generate a reset token
                    string token = Guid.NewGuid().ToString();

                    // Save token and expiry time to the user/supplier record
                    if (user != null)
                    {
                        user.ResetToken = token;
                        user.ResetTokenExpiry = DateTime.Now.AddHours(1);
                    }
                    else if (supplier != null)
                    {
                        supplier.ResetToken = token;
                        supplier.ResetTokenExpiry = DateTime.Now.AddHours(1);
                    }

                    db_context.SaveChanges();

                    // Send an email with the reset link
                    string resetLink = Url.Action("ResetPassword", "Account", new { token = token }, Request.Url.Scheme);
                    SendEmail(model.Email, "Salasanan nollaus, Suosittelija.fi sivuille", $"Voit nollata salasanasi tästä linkistä: <a href='{resetLink}'>Paina tästä!</a>");

                    return RedirectToAction("ForgotPasswordConfirmation");
                }
                else
                {
                    ModelState.AddModelError("", "Sähköpostia ei löytynyt!");
                }
            }

            return View(model);
        }

        // GET: Account/ForgotPasswordConfirmation
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // GET: Account/ResetPassword
        public ActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        // POST: Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = db_context.Users.FirstOrDefault(u => u.ResetToken == model.Token && u.ResetTokenExpiry > DateTime.Now);
                var supplier = db_context.Suppliers.FirstOrDefault(s => s.ResetToken == model.Token && s.ResetTokenExpiry > DateTime.Now);

                if (user != null || supplier != null)
                {
                    var hashedPassword = HashPassword(model.PasswordHash);

                    if (user != null)
                    {
                        user.PasswordHash = hashedPassword;
                        user.ResetToken = null;
                        user.ResetTokenExpiry = null;
                    }
                    else if (supplier != null)
                    {
                        supplier.PasswordHash = hashedPassword;
                        supplier.ResetToken = null;
                        supplier.ResetTokenExpiry = null;
                    }

                    db_context.SaveChanges();

                    return RedirectToAction("ResetPasswordConfirmation");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid token or token expired.");
                }
            }

            return View(model);
        }

        // GET: Account/ResetPasswordConfirmation
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }



        private void SendEmail(string email, string subject, string message)
        {
            // Disable SSL certificate validation (only for testing or internal use)
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                          System.Security.Cryptography.X509Certificates.X509Chain chain,
                          System.Net.Security.SslPolicyErrors sslPolicyErrors) { return true; };

            var smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
            var smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];

            using (var client = new SmtpClient("mail.wbservice.fi"))
            {
                client.Port = 587;
                client.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPassword);
                client.EnableSsl = true;

                var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("walter@wbservice.fi");
                mailMessage.To.Add(email);
                mailMessage.Subject = subject;
                mailMessage.Body = message;
                mailMessage.IsBodyHtml = true;

                client.Send(mailMessage);
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
