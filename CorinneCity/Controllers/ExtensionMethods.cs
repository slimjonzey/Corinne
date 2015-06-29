using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Mvc;

namespace CorinneCity.Controllers
{
    public static class ExtensionMethods
    {
        public static string ToJSON(this object obj)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            var json = jss.Serialize(obj);
            return json;
        }
    }
}