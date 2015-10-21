using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CorinneCity.Models;
using System.IO;

namespace CorinneCity.Controllers
{
    public class GovernmentController : Controller
    {
        private UsersContext model = new UsersContext();
        private List<string> councilDuties;
        private List<GovOfficialFormatted> formattedOfficials = new List<GovOfficialFormatted>();
        //
        // GET: /Government/

        public ActionResult Council()
        {
            var db = new Entities();
            foreach (GovOfficial official in db.GovOfficials)
            {
                if (!string.IsNullOrEmpty(official.Spiel))
                {
                    GovOfficialFormatted formatted = new GovOfficialFormatted();
                    councilDuties = new List<string>();
                    GetDuties(official.Spiel);
                    formatted.Duties = councilDuties;
                    formatted.Department = official.Department;
                    formatted.Email = official.Email;
                    formatted.Name = official.Name;
                    formatted.Phone = official.Phone;
                    formatted.Photo = official.Photo;
                    formatted.Title = official.Title;
                    formattedOfficials.Add(formatted);
                }
            }
            return View(formattedOfficials);
        }

        private void GetDuties(string duties)
        {
            string remainder = string.Empty;

            if (duties.Contains("|"))
            {
                int separator = duties.IndexOf("|", StringComparison.Ordinal);
                councilDuties.Add(duties.Substring(0, separator));
                remainder = duties.Substring(separator + 1, duties.Length - (separator + 1));
            }
            else
            {
                councilDuties.Add(duties);
            }

            if (!string.IsNullOrEmpty(remainder))
            {
                if (remainder.Contains("|"))
                {
                    GetDuties(remainder);
                }
                else
                {
                    councilDuties.Add(remainder);
                }
            }
        }

        public ActionResult CouncilMinutes()
        {
            //List<Minute> councilMinutes = new List<Minute>();
            List<Minute> newYear = null;
            Dictionary<string, List<Minute>> sortedMinutes = new Dictionary<string, List<Minute>>();
            List<string> yearsUsed = new List<string>();
            using (var db = new Entities())
            {
                foreach (Minute content in db.Minutes)
                {
                    if (content.FileType == 1)
                    {
                        var stream = new StreamReader(new MemoryStream(content.FileBytes));
                        content.FileContent = stream.ReadToEnd();
                        content.FileContent = content.FileContent.Replace("\r\n", "<br />");
                        content.FileContent = content.FileContent.Replace("\r", "<br />");
                        content.FileContent = content.FileContent.Replace("\n", "");
                        content.FileContent = content.FileContent.Replace("'", "");
                        content.FileContent = content.FileContent.Replace("\0", "");
                        if (!yearsUsed.Contains(content.Year))
                        {
                            newYear = new List<Minute>();
                            sortedMinutes.Add(content.Year, newYear);
                            newYear.Add(content);
                            yearsUsed.Add(content.Year);
                        }
                        else
                        {
                            newYear.Add(content);
                        }
                    }
                }
                return View(sortedMinutes);
            }
        }

        public ActionResult PlanMinutes()
        {
            //List<Minute> planMinutes = new List<Minute>();
            List<Minute> newYear = null;
            Dictionary<string, List<Minute>> sortedMinutes = new Dictionary<string, List<Minute>>();
            List<string> yearsUsed = new List<string>();
            using (var db = new Entities())
            {
                foreach (Minute content in db.Minutes)
                {
                    if (content.FileType == 2)
                    {
                        var stream = new StreamReader(new MemoryStream(content.FileBytes));
                        content.FileContent = stream.ReadToEnd();
                        content.FileContent = content.FileContent.Replace("\r\n", "<br />");
                        content.FileContent = content.FileContent.Replace("\r", "<br />");
                        content.FileContent = content.FileContent.Replace("\n", "");
                        content.FileContent = content.FileContent.Replace("'", "");
                        content.FileContent = content.FileContent.Replace("\0", "");
                        if (!yearsUsed.Contains(content.Year))
                        {
                            newYear = new List<Minute>();
                            sortedMinutes.Add(content.Year, newYear);
                            newYear.Add(content);
                            yearsUsed.Add(content.Year);
                        }
                        else
                        {
                            newYear.Add(content);
                        }
                    }
                }
                return View(sortedMinutes);
            }
        }

        public ActionResult PlanZone()
        {
            var db = new Entities();
            return View(db.GovOfficials.ToList());
        }

        public ActionResult Fire()
        {
            var db = new Entities();
            return View(db.GovOfficials.ToList());
        }

        public ActionResult Employees()
        {
            var db = new Entities();
            return View(db.GovOfficials.ToList());
        }

        public ActionResult Adjustments()
        {
            var db = new Entities();
            return View(db.GovOfficials.ToList());
        }

        public ActionResult Cemetery()
        {
            var db = new Entities();
            return View(db.GovOfficials.ToList());
        }

        public ActionResult CityCodes()
        {
            return View();
        }

    }
}
