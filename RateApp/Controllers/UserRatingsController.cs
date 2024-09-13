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
        public ActionResult SubmitRating(UserRatingViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate OTP
                var otpEntry = db.RatingOTPs.FirstOrDefault(o => o.OTP == model.OTP
                     && o.UserId == (int)model.RatedUserId
                     && o.IsUsed == false
                     && o.ExpiresAt > DateTime.Now);

                if (otpEntry == null)
                {
                    ModelState.AddModelError("", "Invalid or expired OTP.");
                    return View(model);
                }

                // OTP is valid, proceed with rating
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Login", "Account"); // Ensure the user is logged in
                }

                int raterId = (int)Session["UserId"]; // Get logged-in user's ID

                var userRating = new UserRatings
                {
                    RatedUserId = model.RatedUserId,
                    RaterId = raterId, // Assign the logged-in user as the rater
                    RatingValue = model.RatingValue,
                    Comment = model.Comment,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                db.UserRatings.Add(userRating);
                db.SaveChanges();

                // Mark OTP as used
                otpEntry.IsUsed = true;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.RatedUserId = new SelectList(db.Users, "UserId", "UserName", model.RatedUserId);
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
