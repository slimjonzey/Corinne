using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CorinneCity.Controllers
{
    public class ServicesController : Controller
    {
        //
        // GET: /Services/

        public ActionResult Index()
        {
            ViewBag.Message = "Please print this page and return it to City Hall";
            return View();
        }

        public ActionResult AddDog()
        {
            ViewBag.Message = "Please print this page and return it to City Hall";
            return View();
        }

        public ActionResult Building()
        {
            ViewBag.Message = "Please print this page and return it to City Hall";
            return View();
        }

        public ActionResult Business()
        {
            ViewBag.Message = "Please print this page and return it to City Hall";
            return View();
        }

        public ActionResult ConditionalUse()
        {
            ViewBag.Message = "Please print this page and return it to City Hall";
            return View();
        }

        public ActionResult Reservations()
        {
            ViewBag.Message = "Please print this page and return it to City Hall";
            return View();
        }

        public ActionResult DogRenew()
        {
            ViewBag.Message = "Please print this page and return it to City Hall";
            return View();
        }

        public ActionResult Excavation()
        {
            ViewBag.Message = "Please print this page and return it to City Hall";
            return View();
        }

        public ActionResult VetCheck()
        {
            ViewBag.Message = "Please print this page and return it to City Hall";
            return View();
        }
    }
}
