﻿using System;
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
    public class SupplierRatingsController : Controller
    {
        private RatingDBEntities1 db = new RatingDBEntities1();

        // GET: SupplierRatings
        public ActionResult Index()
        {
            var supplierRatings = db.SupplierRatings.Include(s => s.Users).Include(s => s.Suppliers);
            return View(supplierRatings.ToList());
        }

        // GET: SupplierRatings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SupplierRatings supplierRatings = db.SupplierRatings
                .Include(s => s.Users)  // Assuming Users is the rater
                .Include(s => s.Suppliers)
                .FirstOrDefault(s => s.RatingId == id);

            if (supplierRatings == null)
            {
                return HttpNotFound();
            }
            return View(supplierRatings);
        }

        // GET: SupplierRatings/Create
        public ActionResult Create()
        {
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
                    ModelState.AddModelError("", "Invalid or expired OTP.");
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

                System.Diagnostics.Debug.WriteLine("Rating successfully saved!");

                return RedirectToAction("Index");
            }

            return View(model);
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
