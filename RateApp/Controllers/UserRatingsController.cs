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
    public class UserRatingsController : Controller
    {
        private RatingDBEntities1 db = new RatingDBEntities1();

        // GET: UserRatings
        public ActionResult Index()
        {
            var userRatings = db.UserRatings.Include(u => u.Users).Include(u => u.Users1);
            return View(userRatings.ToList());
        }

        // GET: UserRatings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserRatings userRatings = db.UserRatings.Find(id);
            if (userRatings == null)
            {
                return HttpNotFound();
            }
            return View(userRatings);
        }

        // GET: UserRatings/Create
        public ActionResult Create()
        {
            ViewBag.RaterId = new SelectList(db.Users, "UserId", "UserName");
            ViewBag.UserId = new SelectList(db.Users, "UserId", "UserName");
            return View();
        }



        // POST: UserRatings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserRatingViewModel model)
        {
            // Debug: Log all validation errors
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                foreach (var error in state.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
                }
            }

            System.Diagnostics.Debug.WriteLine("ModelState.IsValid: " + ModelState.IsValid);

            // Check if ModelState is valid
            if (ModelState.IsValid)
            {
                // Check if the supplier is logged in
                if (Session["SupplierId"] == null)
                {
                    System.Diagnostics.Debug.WriteLine("Supplier is not logged in. Redirecting to login.");
                    // Redirect to login page if the supplier is not logged in
                    return RedirectToAction("Login", "Account");
                }

                // Fetch supplier ID from session
                int raterId = 0;
                if (Session["SupplierId"] != null)
                {
                    raterId = Convert.ToInt32(Session["SupplierId"]);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Rater ID is missing in session.");
                    return RedirectToAction("Login", "Account");
                }

                System.Diagnostics.Debug.WriteLine($"Rater ID (Supplier) from session: {raterId}");

                // Validate OTP
                var otpEntry = db.RatingOTPs.FirstOrDefault(o => o.OTP == model.OTP
                             && o.IsUsed == false
                             && o.ExpiresAt > DateTime.Now);

                if (otpEntry == null)
                {
                    System.Diagnostics.Debug.WriteLine("Invalid or expired OTP.");
                    ModelState.AddModelError("", "Invalid or expired OTP.");
                    return View(model);
                }

                // OTP is valid, proceed with rating
                int userId = otpEntry.UserId ?? 0; // Get the user ID from the OTP
                if (userId == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Invalid user ID.");
                    ModelState.AddModelError("", "Invalid user ID.");
                    return View(model);
                }

                // Debug: Check rating details before saving
                System.Diagnostics.Debug.WriteLine("Creating user rating...");
                System.Diagnostics.Debug.WriteLine($"UserId: {userId}");
                System.Diagnostics.Debug.WriteLine($"RaterId: {raterId}");
                System.Diagnostics.Debug.WriteLine($"RatingValue: {model.RatingValue}");
                System.Diagnostics.Debug.WriteLine($"Comment: {model.Comment}");

                // Create a new UserRatings object
                var userRating = new UserRatings
                {
                    UserId = userId,
                    RaterId = raterId,  // Set the RaterId based on the logged-in supplier
                    RatingValue = model.RatingValue,
                    Comment = model.Comment,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                try
                {
                    // Add the rating to the database
                    db.UserRatings.Add(userRating);
                    db.SaveChanges(); // Attempt to save to the database
                    System.Diagnostics.Debug.WriteLine("User rating successfully saved to the database.");

                    // Mark OTP as used
                    otpEntry.IsUsed = true;
                    db.SaveChanges(); // Save the change to OTP status

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Log any exceptions that occur during the SaveChanges call
                    System.Diagnostics.Debug.WriteLine("Error during SaveChanges: " + ex.Message);
                    System.Diagnostics.Debug.WriteLine("Stack Trace: " + ex.StackTrace);
                    ModelState.AddModelError("", "Error saving the rating. Please try again.");
                }
            }

            // If ModelState is not valid or saving fails, return the form with errors
            return View(model);
        }







        // GET: UserRatings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserRatings userRatings = db.UserRatings.Find(id);
            if (userRatings == null)
            {
                return HttpNotFound();
            }
            ViewBag.RaterId = new SelectList(db.Users, "UserId", "UserName", userRatings.RaterId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "UserName", userRatings.UserId);
            return View(userRatings);
        }

        // POST: UserRatings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "RatingId,RaterId,UserId,RatingValue,Comment,CreatedAt,UpdatedAt")] UserRatings userRatings)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userRatings).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.RaterId = new SelectList(db.Users, "UserId", "UserName", userRatings.RaterId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "UserName", userRatings.UserId);
            return View(userRatings);
        }

        // GET: UserRatings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserRatings userRatings = db.UserRatings.Find(id);
            if (userRatings == null)
            {
                return HttpNotFound();
            }
            return View(userRatings);
        }

        // POST: UserRatings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UserRatings userRatings = db.UserRatings.Find(id);
            db.UserRatings.Remove(userRatings);
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
