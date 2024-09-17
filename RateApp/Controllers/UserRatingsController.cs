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
            // Include the related rater (which is a user)
            UserRatings userRatings = db.UserRatings
                                        .Include(u => u.Users1)  // Assuming Users1 is the supplier/rater
                                        .FirstOrDefault(u => u.RatingId == id);

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


            // Check if ModelState is valid
            if (ModelState.IsValid)
            {
                // Check if the supplier is logged in
                if (Session["SupplierId"] == null)
                {
                    // Redirect to login page if the supplier is not logged in
                    return RedirectToAction("Login", "Account");
                }

                // Fetch supplier ID from session
                int raterId = 0;
                if (Session["SupplierId"] != null)
                {
                    raterId = Convert.ToInt32(Session["SupplierId"]);
                }

                // Validate OTP
                var otpEntry = db.RatingOTPs.FirstOrDefault(o => o.OTP == model.OTP
                             && o.IsUsed == false
                             && o.ExpiresAt > DateTime.Now);

                if (otpEntry == null)
                {
                    ModelState.AddModelError("", "Invalid or expired OTP.");
                    return View(model);
                }

                // OTP is valid, proceed with rating
                int userId = otpEntry.UserId ?? 0; // Get the user ID from the OTP
                if (userId == 0)
                {
                    ModelState.AddModelError("", "Invalid user ID.");
                    return View(model);
                }

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


                    // Add the rating to the database
                db.UserRatings.Add(userRating);
                db.SaveChanges(); // Attempt to save to the database

                   // Mark OTP as used
                otpEntry.IsUsed = true;
                db.SaveChanges(); // Save the change to OTP status

                System.Diagnostics.Debug.WriteLine("User rating successfully saved to the database.");



                return RedirectToAction("Index");

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
