using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PhiBook.Web.Models;
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

                               return View["index", contacts];
                           };

            Get["/{id}"] = parameters =>
                               {
                                   var contact = ravenSession.Load<Contact>(parameters.id);
                                   return View["edit", contact];
                               };

            Get["/create"] = parameters => View["create", new Contact()];

            Post["/"] = parameters =>
                            {
                                var newContact = new Contact();
                                // need to persist to raven and return
                                return View["edit", newContact];
                            };
        }
    }
}