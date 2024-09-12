using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RateApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UserId"] != null || Session["SupplierId"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

            public ActionResult About()
        {
            ViewBag.Message = "Meistä";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Yhteystiedot";

            return View();
        }
    }
}