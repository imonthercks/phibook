using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhiBook.Web.Models
{
    public class Contact
    {
        public string Id { get; set; }

        public string MemberId { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MiddleInitial { get; set; }

        public DateTime? DateOfDeath { get; set; }

        public string ConfirmedPhoneNumbers { get; set; }

        public string ConfirmedEmailAddresses { get; set; }

        public ContactAddress ConfirmedMailingAddress { get; set; }

        public ContactAddress[] AllAddresses { get; set; }

        public ContactEmail[] AllEmailAddresses { get; set; }

        public ContactPhone[] AllPhoneNumbers { get; set; }
    }
}