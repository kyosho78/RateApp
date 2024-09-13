using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RateApp.Models;

namespace RateApp.Controllers
{
    public class RatingOTPsController : Controller
    {
        private RatingDBEntities1 db = new RatingDBEntities1();

        // GET: RatingOTPs
        public ActionResult Index()
        {
            var ratingOTPs = db.RatingOTPs.Include(r => r.Suppliers).Include(r => r.Users);
            return View(ratingOTPs.ToList());
        }

        // GET: RatingOTPs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RatingOTPs ratingOTPs = db.RatingOTPs.Find(id);
            if (ratingOTPs == null)
            {
                return HttpNotFound();
            }
            return View(ratingOTPs);
        }

        // GET: RatingOTPs/Create
        public ActionResult Create()
        {
            ViewBag.SupplierId = new SelectList(db.Suppliers, "SupplierId", "SupplierName");
            ViewBag.UserId = new SelectList(db.Users, "UserId", "UserName");
            return View();
        }

        public ActionResult GenerateOTP(int? userId = null, int? supplierId = null)
        {
            // Check if the current session is a supplier or user
            if (Session["SupplierId"] != null)
            {
                supplierId = (int)Session["SupplierId"];  // Use SupplierId from the session
                userId = null;  // Ensure UserId is null when generating OTP for a supplier
            }
            else if (Session["UserId"] != null)
            {
                userId = (int)Session["UserId"];  // Use UserId from the session
                supplierId = null;  // Ensure SupplierId is null when generating OTP for a user
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "User or Supplier ID is required.");
            }

            // Generate a random 6-digit OTP
            var random = new Random();
            string otp = random.Next(100000, 999999).ToString();

            // Save the OTP in the database with expiration time
            var newOTP = new RatingOTPs
            {
                UserId = userId.HasValue ? userId.Value : (int?)null,  // Only set UserId if it's not null
                SupplierId = supplierId.HasValue ? supplierId.Value : (int?)null,  // Only set SupplierId if it's not null
                OTP = otp,
                GeneratedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(1440),  // OTP expires in 1440 minutes, one day
                IsUsed = false
            };

            db.RatingOTPs.Add(newOTP);
            db.SaveChanges();

            // Pass the generated OTP to the view using ViewBag
            ViewBag.GeneratedOTP = otp;

            // Return the view to display the OTP
            return View("GenerateOTP");
        }







        public ActionResult ValidateOTP(string otp, int supplierId)
        {
            var otpEntry = db.RatingOTPs.FirstOrDefault(o => o.OTP == otp && o.SupplierId == supplierId && o.IsUsed == false && o.ExpiresAt > DateTime.Now);

            if (otpEntry != null)
            {
                // OTP is valid, mark it as used
                otpEntry.IsUsed = true;
                db.SaveChanges();
                return Content("OTP is valid.");
            }
            else
            {
                return Content("OTP is invalid or expired.");
            }
        }





        // GET: RatingOTPs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RatingOTPs ratingOTPs = db.RatingOTPs.Find(id);
            if (ratingOTPs == null)
            {
                return HttpNotFound();
            }
            ViewBag.SupplierId = new SelectList(db.Suppliers, "SupplierId", "SupplierName", ratingOTPs.SupplierId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "UserName", ratingOTPs.UserId);
            return View(ratingOTPs);
        }

        // POST: RatingOTPs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OTPId,UserId,SupplierId,OTP,GeneratedAt,ExpiresAt,IsUsed")] RatingOTPs ratingOTPs)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ratingOTPs).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.SupplierId = new SelectList(db.Suppliers, "SupplierId", "SupplierName", ratingOTPs.SupplierId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "UserName", ratingOTPs.UserId);
            return View(ratingOTPs);
        }

        // GET: RatingOTPs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RatingOTPs ratingOTPs = db.RatingOTPs.Find(id);
            if (ratingOTPs == null)
            {
                return HttpNotFound();
            }
            return View(ratingOTPs);
        }

        // POST: RatingOTPs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RatingOTPs ratingOTPs = db.RatingOTPs.Find(id);
            db.RatingOTPs.Remove(ratingOTPs);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
