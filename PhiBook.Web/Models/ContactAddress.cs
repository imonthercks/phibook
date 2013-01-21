﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhiBook.Web.Models
{
    public class ContactAddress
    {
        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string PostalCode { get; set; }

        public string Country { get; set; }

        public string Source { get; set; }

        public DateTime Updated { get; set; }

    }
}