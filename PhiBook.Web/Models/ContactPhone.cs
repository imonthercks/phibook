using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhiBook.Web.Models
{
    public class ContactPhone
    {
        public string Phone { get; set; }

        public string PhoneType { get; set; }

        public string Source { get; set; }

        public DateTime Updated { get; set; }
    }
}