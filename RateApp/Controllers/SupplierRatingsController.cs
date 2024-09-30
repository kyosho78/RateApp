using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using RateApp.Models;
using Rotativa;

namespace RateApp.Controllers
{
    public class SupplierRatingsController : Controller
    {
        private RatingDBEntities1 db = new RatingDBEntities1();

        // GET: SupplierRatings
        public ActionResult Index()
        {
            // Check if the SupplierId exists in the session
            if (Session["UserId"] == null)
            {
                // Redirect to login page or show an error if the supplier is not logged in
                return RedirectToAction("Login", "Account");  // Redirect to the login page or any appropriate action
            }

            // Retrieve the SupplierId from the session
            int userId = (int)Session["UserId"];

            // Filter the supplierRatings by the logged-in supplier's ID
            var supplierRatings = db.SupplierRatings
                                    .Include(s => s.Users)
                                    .Include(s => s.Suppliers)
                                    .Where(s => s.SupplierId == userId)
                                    .ToList();

            // Check if there are any ratings
            if (!supplierRatings.Any())
            {
                ViewBag.Message = "Ei arvosteluita!";
            }

            return View(supplierRatings);
        }

        // GET: SupplierRatings/Details
        public ActionResult Details()
        {
            // Check if the SupplierId exists in the session
            if (Session["SupplierId"] == null)
            {
                // Redirect to login page or show an error if the supplier is not logged in
                return RedirectToAction("Login", "Account");
            }

            // Retrieve the SupplierId from the session
            int supplierId = (int)Session["SupplierId"];

            // Filter the supplierRatings by the logged-in supplier's ID
            var supplierRatings = db.SupplierRatings
                                    .Include(s => s.Users)
                                    .Include(s => s.Suppliers)
                                    .Where(s => s.SupplierId == supplierId)
                                    .ToList();

            // Check if there are any ratings
            if (!supplierRatings.Any())
            {
                ViewBag.Message = "Ei arvosteluita!";
            }

            return View(supplierRatings);  // Return the list of ratings
        }


        // GET: SupplierRatings/Create
        public ActionResult Create()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.RaterId = new SelectList(db.Users, "UserId", "Username");
            ViewBag.SupplierId = new SelectList(db.Suppliers, "SupplierId", "SupplierName");
            return View();
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SupplierRatingViewModel model)
        {
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                foreach (var error in state.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
                }
            }

            if (ModelState.IsValid)
            {
                // Check if the user is logged in
                if (Session["UserId"] == null)
                {
                    // Redirect to login page if the user is not logged in
                    return RedirectToAction("Login", "Account");
                }

                // Validate OTP and fetch SupplierId
                var otpEntry = db.RatingOTPs.FirstOrDefault(o => o.OTP == model.OTP
                                && o.IsUsed == false
                                && o.ExpiresAt > DateTime.Now);

                if (otpEntry == null)
                {
                    ModelState.AddModelError("OTP", "Väärä koodi. Tarkista onko sinulla oikea koodi!");
                    return View(model); // Show error on the view
                }

                // OTP is valid, retrieve the supplier ID
                int supplierId = otpEntry.SupplierId ?? 0;
                if (supplierId == 0)
                {
                    ModelState.AddModelError("", "Invalid Supplier.");
                    return View(model);
                }

                int raterId = (int)Session["UserId"];  // Fetch user ID from session

                var supplierRating = new SupplierRatings
                {
                    SupplierId = supplierId,
                    RaterId = raterId,  // Set the RaterId based on the current logged-in user
                    RatingValue = model.RatingValue,
                    Comment = model.Comment,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                db.SupplierRatings.Add(supplierRating);
                db.SaveChanges();

                // Mark OTP as used
                otpEntry.IsUsed = true;
                db.SaveChanges();

                // Set a success message in TempData
                TempData["SuccessMessage"] = "Kiitos arvostelusta!";

                return RedirectToAction("Success");
            }

            return View(model);
        }


        public ActionResult Success()
        {
            return View();
        }








        // GET: SupplierRatings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SupplierRatings supplierRatings = db.SupplierRatings.Find(id);
            if (supplierRatings == null)
            {
                return HttpNotFound();
            }

            var model = new SupplierRatingViewModel
            {
                SupplierId = supplierRatings.SupplierId,
                RatingValue = supplierRatings.RatingValue,
                Comment = supplierRatings.Comment,
                RaterId = supplierRatings.RaterId  // Add RaterId to the model
            };

            ViewBag.SupplierId = new SelectList(db.Suppliers, "SupplierId", "SupplierName", supplierRatings.SupplierId);
            return View(model);
        }


        // POST: SupplierRatings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, SupplierRatingViewModel model)
        {
            if (ModelState.IsValid)
            {
                var supplierRatings = db.SupplierRatings.Find(id);
                if (supplierRatings == null)
                {
                    return HttpNotFound();
                }

                // Update only editable fields, keeping RaterId unchanged
                supplierRatings.SupplierId = model.SupplierId;
                supplierRatings.RatingValue = model.RatingValue;
                supplierRatings.Comment = model.Comment;
                supplierRatings.UpdatedAt = DateTime.Now;

                db.Entry(supplierRatings).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.SupplierId = new SelectList(db.Suppliers, "SupplierId", "SupplierName", model.SupplierId);
            return View(model);
        }



        // GET: SupplierRatings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SupplierRatings supplierRatings = db.SupplierRatings.Find(id);
            if (supplierRatings == null)
            {
                return HttpNotFound();
            }
            return View(supplierRatings);
        }

        // POST: SupplierRatings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SupplierRatings supplierRatings = db.SupplierRatings.Find(id);
            db.SupplierRatings.Remove(supplierRatings);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult DownloadIndexAsPdf()
        {
            // Fetch all ratings for the current user (adjust user ID retrieval based on your authentication method)
            var supplierId = Convert.ToInt32(Session["SupplierId"]); // Get the current user ID from session or context

            // Ensure that `userId` is correctly defined. If you don't have a user context, remove this filter.
            List<SupplierRatings> allRatings = db.SupplierRatings
                .Include(r => r.Suppliers)  // Including related suppliers
                .Include(u => u.Users)  // Including related users
                .Where(r => r.SupplierId == supplierId)  // Fetch ratings for the logged-in user
                .ToList();

            // Check if there are no ratings and return a 404 error if none exist
            if (allRatings == null || allRatings.Count == 0)
            {
                return HttpNotFound("No ratings found.");
            }

            // Use Rotativa to return the Index view as a PDF
            return new ViewAsPdf("Index", allRatings)
            {
                FileName = "SupplierRatings.pdf"
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
