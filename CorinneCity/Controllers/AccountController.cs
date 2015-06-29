using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using CorinneCity.Filters;
using CorinneCity.Models;
using System.Text;
using Novacode;

namespace CorinneCity.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                return RedirectToAction("Manage", "Account");
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
                    WebSecurity.Login(model.UserName, model.Password);
                    return RedirectToAction("Index", "Home");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        [HttpPost]
        public ActionResult RemoveCalendarItem(List<EventBoolConverter> events)
        {
            using (var db = new Entities())
            {
                foreach (EventBoolConverter delEvent in events)
                {
                    if (delEvent.Selected == true)
                    {
                        CalendarEvent update = new CalendarEvent();
                        update.Details = delEvent.Details;
                        update.End = delEvent.End;
                        update.Id = delEvent.Id;
                        update.Location = delEvent.Location;
                        update.Start = delEvent.Start;
                        update.Title = delEvent.Title;
                        db.CalendarEvents.Attach(update);
                        db.Entry(update).State = System.Data.EntityState.Deleted;
                        db.SaveChanges();
                    }
                }
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.CalendarDeleteSuccess });
        }

        [HttpPost]
        public ActionResult UploadCouncilMinutes(HttpPostedFileBase file, string year, int fileType)
        {
            string serverPath = string.Empty;
            Minute newMinutes = new Minute();
            newMinutes.FileType = fileType;
            foreach (string upload in Request.Files)
            {
                serverPath = AppDomain.CurrentDomain.BaseDirectory + "Uploads\\";
                newMinutes.FileName = System.IO.Path.GetFileName(Request.Files[upload].FileName);
                Request.Files[upload].SaveAs(System.IO.Path.Combine(serverPath, newMinutes.FileName));
            }
            string textFile = SaveAsText(serverPath + newMinutes.FileName);
            newMinutes.FileBytes = new byte[textFile.Length * sizeof(char)];
            System.Buffer.BlockCopy(textFile.ToCharArray(), 0, newMinutes.FileBytes, 0, newMinutes.FileBytes.Length);
            System.IO.File.Delete(serverPath + newMinutes.FileName);
            newMinutes.FileContent = string.Empty;
            newMinutes.Year = year.Substring(0, 4);
            using (var entities = new Entities())
            {
                entities.Minutes.Add(newMinutes);
                entities.SaveChanges();
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.MinutesUploadSuccess });
        }

        public ActionResult UploadPlanZoneMinutes(HttpPostedFileBase file, string year, int fileType)
        {
            string serverPath = string.Empty;
            Minute newMinutes = new Minute();
            newMinutes.FileType = fileType;
            foreach (string upload in Request.Files)
            {
                serverPath = AppDomain.CurrentDomain.BaseDirectory + "Uploads\\";
                newMinutes.FileName = System.IO.Path.GetFileName(Request.Files[upload].FileName);
                Request.Files[upload].SaveAs(System.IO.Path.Combine(serverPath, newMinutes.FileName));
            }
            string textFile = SaveAsText(serverPath + newMinutes.FileName);
            newMinutes.FileBytes = new byte[textFile.Length * sizeof(char)];
            System.Buffer.BlockCopy(textFile.ToCharArray(), 0, newMinutes.FileBytes, 0, newMinutes.FileBytes.Length);
            System.IO.File.Delete(serverPath + newMinutes.FileName);
            newMinutes.FileContent = string.Empty;
            newMinutes.Year = year.Substring(0, 4);
            using (var entities = new Entities())
            {
                entities.Minutes.Add(newMinutes);
                entities.SaveChanges();
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.MinutesUploadSuccess });
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
            message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
            : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
            : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
            : message == ManageMessageId.MinutesUploadSuccess ? "The minutes have been uploaded successfully."
            : message == ManageMessageId.CalendarUploadSuccess ? "The calendar event has been uploaded successfully."
            : message == ManageMessageId.CalendarDeleteSuccess ? "The calendar event(s) have been deleted successfully."
            : message == ManageMessageId.UserAddedSuccess ? "The new user has been added successfully."
            : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model, string title, string date, string start, string end, bool? allDay, string location, string description, string year, bool? register, RegisterModel rm, string removeCalendar, string councilMinutes, string planMinutes)
        {
            if (!string.IsNullOrEmpty(councilMinutes))
            {
                return View("UploadCouncilMinutes");
            }
            else if (!string.IsNullOrEmpty(planMinutes))
            {
                return View("UploadPlanZoneMinutes");
            }
            else if (!string.IsNullOrEmpty(title))
            {
                CalendarEvent newEvent = new CalendarEvent();
                if (start.Contains("AM"))
                    start = start.Replace("AM", string.Empty).Trim();
                else if (start.Contains("PM"))
                {
                    int colonSpot = start.IndexOf(":");
                    int oldStartHour = Convert.ToInt32(start.Substring(0, colonSpot));
                    int newStartHour = 0;
                    if (oldStartHour != 12)
                    {
                        newStartHour = oldStartHour + 12;
                    }
                    else
                    {
                        newStartHour = oldStartHour;
                    }
                    start = start.Replace("PM", string.Empty).Trim();
                    start = start.Replace(oldStartHour.ToString(), newStartHour.ToString());
                }
                start = date + " " + start;
                if (end.Contains("AM"))
                    end = end.Replace("AM", string.Empty);
                else if (end.Contains("PM"))
                {
                    int colonSpot = end.IndexOf(":");
                    int oldEndHour = Convert.ToInt32(end.Substring(0, colonSpot));
                    int newEndHour = 0;
                    if (oldEndHour != 12)
                    {
                        newEndHour = oldEndHour + 12;
                    }
                    else
                    {
                        newEndHour = oldEndHour;
                    }
                    end = end.Replace("PM", string.Empty);
                    end = end.Replace(oldEndHour.ToString(), newEndHour.ToString());
                }
                end = date + " " + end;
                newEvent.Title = title;
                newEvent.Start = start;
                newEvent.End = end;
                newEvent.Location = location;
                newEvent.Details = description;
                using (var entities = new Entities())
                {
                    entities.CalendarEvents.Add(newEvent);
                    entities.SaveChanges();
                }
                return RedirectToAction("Manage", new { Message = ManageMessageId.CalendarUploadSuccess });
            }
            else if (register != null)
            {
                return View("Register", new RegisterModel());
            }
            //assuming that the delete calendar event button was clicked
            else if (!string.IsNullOrEmpty(removeCalendar))
            {
                List<EventBoolConverter> convertedEvents = new List<EventBoolConverter>();
                using (var db = new Entities())
                {
                    foreach (CalendarEvent calEvent in db.CalendarEvents)
                    {
                        EventBoolConverter convert = new EventBoolConverter();
                        convert.Details = calEvent.Details;
                        convert.End = calEvent.End;
                        convert.Id = calEvent.Id;
                        convert.Location = calEvent.Location;
                        if (calEvent.Selected == 0)
                            convert.Selected = false;
                        else
                            convert.Selected = true;
                        convert.Start = calEvent.Start;
                        convert.Title = calEvent.Title;
                        convertedEvents.Add(convert);
                    }
                    return View("RemoveCalendarItem", convertedEvents);
                }
            }
            else if (rm != null)
            {
                // Attempt to register the user
                try
                {
                    WebSecurity.CreateUserAndAccount(rm.UserName, rm.Password);
                    return RedirectToAction("Manage", new { Message = ManageMessageId.UserAddedSuccess });
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }
            else
            {
                bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                ViewBag.HasLocalPassword = hasLocalAccount;
                ViewBag.ReturnUrl = Url.Action("Manage");
                if (hasLocalAccount)
                {
                    if (ModelState.IsValid)
                    {
                        // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                        bool changePasswordSucceeded;
                        try
                        {
                            changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                        }
                        catch (Exception)
                        {
                            changePasswordSucceeded = false;
                        }

                        if (changePasswordSucceeded)
                        {
                            return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                        }
                        else
                        {
                            ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                        }
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public string SaveAsText(string file)
        {
            DocX doc = DocX.Load(file);
            StringBuilder sb = new StringBuilder();

            foreach (Paragraph paragraph in doc.Paragraphs)
            {
                sb.Append(paragraph.Text);
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        public string GetText(string fileName)
        {
            string retValue = string.Empty;
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            if (System.IO.File.Exists(fileName))
            {
                try
                {
                    using (System.IO.StreamReader sr = System.IO.File.OpenText(fileName))
                    {
                        string s = string.Empty;
                        while ((s = sr.ReadLine()) != null)
                        {
                            builder.AppendLine(s);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //do nothing for now
                }
            }
            if (!String.IsNullOrEmpty(builder.ToString()))
            {
                retValue = builder.ToString();
            }
            return retValue;
        }

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (UsersContext db = new UsersContext())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // Check if user already exists
                    if (user == null)
                    {
                        // Insert name into the profile table
                        db.UserProfiles.Add(new UserProfile { UserName = model.UserName });
                        db.SaveChanges();

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "User name already exists. Please enter a different user name.");
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            MinutesUploadSuccess,
            CalendarUploadSuccess,
            CalendarDeleteSuccess,
            UserAddedSuccess
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }

    public class EventBoolConverter
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string Location { get; set; }
        public string Details { get; set; }
        public bool Selected { get; set; }
    }
}
