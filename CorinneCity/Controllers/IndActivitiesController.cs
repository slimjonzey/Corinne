using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using CorinneCity.Models;

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

	    public ActionResult Basketball()
	    {
		    return View();
	    }

        public ActionResult ShowRegForm()
        {
            return File("~/Content/5k release waiver.pdf", "application/pdf", "5k release waiver.pdf");
        }

	    public ActionResult Easter()
	    {
			var easterDate = new EasterDateObject();
			var db = new Entities();
			var calendarEvents = db.CalendarEvents.ToList();
		    var easterEvent = calendarEvents.LastOrDefault(e => e.Title.ToLowerInvariant().Contains("Easter Egg Hunt".ToLowerInvariant()));
		    if (easterEvent != null)
		    {
			    DateTime tempDateTime;
			    if (DateTime.TryParse(easterEvent.Start, out tempDateTime))
			    {
				    easterDate.Date = tempDateTime.ToLongDateString();
				    easterDate.Time = tempDateTime.ToShortTimeString();
			    }
		    }
			return View(easterDate);
	    }
    }

	public class EasterDateObject
	{
		public string Date { get; set; }
		public string Time { get; set; }
	}
}
