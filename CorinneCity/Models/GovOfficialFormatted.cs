using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CorinneCity.Models
{
    public class GovOfficialFormatted
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public byte[] Photo { get; set; }
        public string Department { get; set; }
        public List<string> Duties { get; set; }
    }
}