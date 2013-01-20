using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhiBook.Web.Models.API
{
    public class Contact
    {
        public string Id { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MiddleInitial { get; set; }

        public string Status { get; set; }

        public DateTime InitiationDate { get; set; }

        public DateTime DateOfDeath { get; set; }

        public string ConfirmedPhoneNumbers { get; set; }

        public string ConfirmedEmailAddresses { get; set; }

        public ContactAddress ConfirmedMailingAddress { get; set; }

        public IList<ContactAddress> AllAddresses { get; set; }

        public IList<ContactEmail> AllEmailAddresses { get; set; }

        public IList<ContactPhone> AllPhoneNumbers { get; set; }
    }
}