using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RateApp.Models;
using Rotativa;

namespace RateApp.Controllers
{
    public class UserRatingsController : Controller
    {
        private RatingDBEntities1 db = new RatingDBEntities1();

        // GET: UserRatings
        public ActionResult Index()
        {
            // Check if the UserId exists in the session
            if (Session["SupplierId"] == null)
            {
                // Redirect to login page or show an error if the user is not logged in
                return RedirectToAction("Login", "Account");  // Redirect to the login page or any appropriate action
            }

            // Retrieve the UserId from the session
            int SupplierId = (int)Session["SupplierId"];

            // Filter the userRatings by the logged-in user's ID
            var userRatings = db.UserRatings
                                    .Include(s => s.Users)
                                    .Include(s => s.Suppliers)
                                    .Where(s => s.UserId == SupplierId)
                                    .ToList();

            // Check if there are any ratings
            if (!userRatings.Any())
            {
                ViewBag.Message = "Ei arvosteluita!";
            }

            return View(userRatings);
        }

        // GET: UserRatings/Details/5
        public ActionResult Details()
        {
            // Check if the UserId exists in the session
            if (Session["UserId"] == null)
            {
                // Redirect to login page or show an error if the user is not logged in
                return RedirectToAction("Login", "Account");
            }

            // Retrieve the SupplierId from the session
            int userId = (int)Session["UserId"];

            // Filter the userRatings by the logged-in user's ID
            var userRatings = db.UserRatings
                                    .Include(s => s.Users)
                                    .Include(s => s.Suppliers)
                                    .Where(s => s.UserId == userId)
                                    .ToList();

            // Check if there are any ratings
            if (!userRatings.Any())
            {
                ViewBag.Message = "Ei arvosteluita!";
            }

            return View(userRatings);  // Return the list of ratings
        }

        // GET: UserRatings/Create
        public ActionResult Create()
        {
            // Check if the supplier is logged in
            if (Session["SupplierId"] == null)
            {
                // Redirect to login page if the supplier is not logged in
                return RedirectToAction("Login", "Account");
            }
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
                    ModelState.AddModelError("OTP", "Väärä koodi. Tarkista onko sinulla oikea koodi!");
                    return View(model);
                }

                // OTP is valid, proceed with rating
                int userId = otpEntry.UserId ?? 0; // Get the user ID from the OTP
                if (userId == 0)
                {
                    ModelState.AddModelError("OTP", "Väärä koodi. Tarkista onko sinulla oikea koodi!");
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

                // Set a success message in TempData
                TempData["SuccessMessage"] = "Kiitos arvostelusta!";

                return RedirectToAction("Success");

            }

            // If ModelState is not valid or saving fails, return the form with errors
            return View(model);
        }


        public ActionResult Success()
        {
            return View();
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

        public ActionResult DownloadIndexAsPdf()
        {
            // Fetch all ratings for the current user (adjust user ID retrieval based on your authentication method)
            var userId = Convert.ToInt32(Session["UserId"]); // Get the current user ID from session or context

            // Ensure that `userId` is correctly defined. If you don't have a user context, remove this filter.
            List<UserRatings> allRatings = db.UserRatings
                .Include(r => r.Suppliers)  // Including related suppliers
                .Include(u => u.Users)  // Including related users
                .Where(r => r.UserId == userId)  // Fetch ratings for the logged-in user
                .ToList();

            // Check if there are no ratings and return a 404 error if none exist
            if (allRatings == null || allRatings.Count == 0)
            {
                return HttpNotFound("No ratings found.");
            }

            // Use Rotativa to return the Index view as a PDF
            return new ViewAsPdf("Index", allRatings)
            {
                FileName = "UserRatings.pdf"
            };
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
