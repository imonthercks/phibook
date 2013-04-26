using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhiBook.Web.Models
{
    public class Member
    {
        public string Id { get; set; }

        public string Status { get; set; }

        public DateTime? InitiationDate { get; set; }

    }
}