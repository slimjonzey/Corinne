using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CorinneCity.Models;
using System.Text;
using System.IO;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Net;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Enums;
using DayPilot.Web.Mvc.Events.Month;
using DayPilot.Web.Mvc.Events.Scheduler;
using System.Data.SqlClient;

namespace CorinneCity.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Gateway to the Golden Spike";          
            var db = new Entities();
            List<FormattedDateTimes> returnEvents = new List<FormattedDateTimes>();
            foreach (var item in db.CalendarEvents)
            {
                DateTime itemDate = Convert.ToDateTime(item.Start);
                int future = DateTime.Compare(itemDate, DateTime.Today);
                if (future > 0)
                {
                    FormattedDateTimes formattedTimes = new FormattedDateTimes(item);
                    returnEvents.Add(formattedTimes);
                }
            }
            returnEvents = returnEvents.OrderBy(x => x.EventDate).ToList();
            return View(returnEvents);
        }

        public ActionResult CityInfo()
        {
            return View();
        }

        public ActionResult Calendar()
        {
            return View();
        }

        public ActionResult BackendMonth()
        {
            return new Dpm().CallBack(this);
        }

        public ActionResult AddEvent()
        {
            return View();
        }

        public ActionResult EventDetails()
        {
            return View();
        }
    }

    public class FormattedDateTimes
    {
        public string EventDate { get; set; }
        public string EventStart { get; set; }
        public string EventEnd { get; set; }
        public string EventTitle { get; set; }
        public string EventLocation { get; set; }
        public string EventDetails { get; set; }

        public FormattedDateTimes(CalendarEvent calEvent)
        {
            int firstSpaceStart = calEvent.Start.IndexOf(" ");
            EventDate = calEvent.Start.Substring(0, firstSpaceStart);
            EventStart = CalculateTime(calEvent.Start, firstSpaceStart);
            int firstSpaceEnd = calEvent.End.IndexOf(" ");
            EventEnd = CalculateTime(calEvent.End, firstSpaceEnd);
            EventTitle = calEvent.Title;
            EventLocation = calEvent.Location;
            EventDetails = calEvent.Details;
        }

        public string CalculateTime(string time, int spaceIndex)
        {
            string temp = string.Empty;
            string timeIndicator = string.Empty;
            temp = time.Substring(spaceIndex, time.Length - spaceIndex).Trim();
            int colonLocation = temp.IndexOf(":");
            int hour = Convert.ToInt32(temp.Substring(0, colonLocation));
            if (hour > 12)
            {
                hour = hour - 12;
                timeIndicator = "PM";
            }
            else if (hour == 12)
            {
                timeIndicator = "PM";
            }
            else
            {
                timeIndicator = "AM";
            }
            return hour.ToString() + temp.Substring(colonLocation, 3) + " " + timeIndicator;
        }
    }

    public class Dpm : DayPilotMonth
    {
        protected override void OnInit(DayPilot.Web.Mvc.Events.Month.InitArgs e)
        {
            UpdateEvents();
        }

        protected override void OnCommand(DayPilot.Web.Mvc.Events.Month.CommandArgs e)
        {
            switch (e.Command)
            {
                case "navigate":
                    DateTime start = (DateTime)e.Data["start"];
                    StartDate = start;
                    using (var db = new Entities())
                    {
                        Events = db.CalendarEvents.ToList();
                    }
                    DataStartField = "Start";
                    DataEndField = "End";
                    DataTextField = "Title";
                    DataIdField = "Id";
                    Update(CallBackUpdateType.EventsOnly);
                    Update(CallBackUpdateType.Full);
                    break;
            }
        }

        protected override void OnEventBubble(DayPilot.Web.Mvc.Events.Month.EventBubbleArgs e)
        {
            using (var db = new Entities())
            {
                foreach (CalendarEvent calEvent in db.CalendarEvents)
                {
                    if (calEvent.Id.ToString() == e.Id)
                    {
                        FormattedDateTimes formatted = new FormattedDateTimes(calEvent);
                        e.BubbleHtml = "Start time: " + formatted.EventStart + "<br /><br />" +
                                       "End time: " + formatted.EventEnd + "<br /><br />" +
                                       "Location: " + formatted.EventLocation + "<br /><br />" +
                                       "Description: " + formatted.EventDetails;
                        break;
                    }
                }
            }
        }

        private void UpdateEvents()
        {
            using (var db = new Entities())
            {
                Events = db.CalendarEvents.ToList();
            }
            DataStartField = "Start";
            DataEndField = "End";
            DataTextField = "Title";
            DataIdField = "Id";

            Update();
        }
    }
}
