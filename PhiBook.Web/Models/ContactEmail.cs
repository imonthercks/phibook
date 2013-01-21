using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhiBook.Web.Models
{
    public class ContactEmail
    {
        public string Email { get; set; }

        public string EmailType { get; set; }

        public string Source { get; set; }

        public DateTime Updated { get; set; }
    }
}