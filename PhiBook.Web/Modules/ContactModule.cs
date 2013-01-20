using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using PhiBook.Web.Models;
using Nancy.ModelBinding;
using Raven.Client;

namespace PhiBook.Web.Modules
{
    public class ContactModule : BaseModule
    {
        public ContactModule(IDocumentSession ravenSession) : base("/contact")
        {
            Get["/"] = parameters =>
                           {
                               var contacts = ravenSession.Query<Contact>()
                                   .OrderBy(x => x.LastName)
                                   .ToList();

                               return Response.AsJson(contacts);
                           };

            Get["/{id}"] = parameters =>
                               {
                                   string contactId = "contact/" + parameters.id;
                                   var contact = ravenSession.Load<Contact>(contactId);
                                   return Response.AsJson(contact);
                               };

            Get["/create"] = parameters => View["create", new Contact()];

            Post["/"] = parameters =>
                            {
                                Contact newContact = this.Bind();

                                //Contact newContact = this.Bind("InitiationDate", "DateOfDeath", "ConfirmedPhoneNumbers", "ConfirmedEmailAddresses", "ConfirmedMailingAddresses",
                                //    "AllAddresses", "AllEmailAddresses", "AllPhoneNumbers");
                                //var newContact = new Contact
                                //                     {
                                //                         Id = "contact/348",
                                //                         LastName = "Russell",
                                //                         FirstName = "Christopher",
                                //                         MiddleInitial = "J",
                                //                         Status = "ALUMNUS",
                                //                         InitiationDate = new DateTime(1994, 1, 15)
                                //                     };
                                // need to persist to raven and return

                                ravenSession.Store(newContact);
                                return Response.AsJson(newContact);
                            };
        }
    }
}