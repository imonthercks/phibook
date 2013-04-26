using System;
using System.Linq;
using Nancy;
using PhiBook.Web.Models;
using Nancy.ModelBinding;
using Raven.Client;

namespace PhiBook.Web.Modules.Api
{
    public class ContactModule : SecureModule
    {
        public ContactModule(IDocumentSession ravenSession) : base("/api/contact")
        {
            Get["/"] = parameters =>
                           {
                               var contacts = ravenSession.Query<Contact>()
                                   .OrderBy(x => x.LastName)
                                   .Take(15)
                                   .ToList();

                               return Response.AsJson(contacts);
                           };

            Get["/{id}"] = parameters =>
                               {
                                   string contactId = "contact/" + parameters.id;
                                   var contact = ravenSession.Load<Contact>(contactId);
                                   return Response.AsJson(contact);
                               };

            Post["/"] = parameters =>
                            {
                                Contact newContact = this.Bind();

                                newContact.DateOfDeath = newContact.DateOfDeath == DateTime.MinValue
                                                             ? null
                                                             : newContact.DateOfDeath;

                                ravenSession.Store(newContact);
                                return Response.AsJson(newContact);
                            };
        }
    }
}