using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;

namespace CorinneCity.Controllers
{
    public class IndActivitiesController : Controller
    {
        //
        // GET: /IndActivities/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Schedule()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        public ActionResult ShowRegForm()
        {
            return File("~/Content/5k release waiver.pdf", "application/pdf", "5k release waiver.pdf");
        }
    }
}
