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
    public class LoginsController : Controller
    {
        private RatingDBEntities db = new RatingDBEntities();

        // GET: Logins
        public ActionResult Index()
        {
            var logins = db.Logins.Include(l => l.Users).Include(l => l.Suppliers).Include(l => l.Users1);
            return View(logins.ToList());
        }

        // GET: Logins/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Logins logins = db.Logins.Find(id);
            if (logins == null)
            {
                return HttpNotFound();
            }
            return View(logins);
        }

        // GET: Logins/Create
        public ActionResult Create()
        {
            ViewBag.UserId = new SelectList(db.Users, "UserId", "UserName");
            ViewBag.SupplierId = new SelectList(db.Suppliers, "SupplierId", "SupplierName");
            ViewBag.UserId = new SelectList(db.Users, "UserId", "UserName");
            return View();
        }

        // POST: Logins/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "LoginId,UserId,LoginTime,LogoutTime,IpAddress,SupplierId")] Logins logins)
        {
            if (ModelState.IsValid)
            {
                db.Logins.Add(logins);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.Users, "UserId", "UserName", logins.UserId);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "SupplierId", "SupplierName", logins.SupplierId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "UserName", logins.UserId);
            return View(logins);
        }

        // GET: Logins/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Logins logins = db.Logins.Find(id);
            if (logins == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.Users, "UserId", "UserName", logins.UserId);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "SupplierId", "SupplierName", logins.SupplierId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "UserName", logins.UserId);
            return View(logins);
        }

        // POST: Logins/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "LoginId,UserId,LoginTime,LogoutTime,IpAddress,SupplierId")] Logins logins)
        {
            if (ModelState.IsValid)
            {
                db.Entry(logins).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.Users, "UserId", "UserName", logins.UserId);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "SupplierId", "SupplierName", logins.SupplierId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "UserName", logins.UserId);
            return View(logins);
        }

        // GET: Logins/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Logins logins = db.Logins.Find(id);
            if (logins == null)
            {
                return HttpNotFound();
            }
            return View(logins);
        }

        // POST: Logins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Logins logins = db.Logins.Find(id);
            db.Logins.Remove(logins);
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
