using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using Nancy.Extensions;
using Nancy.Security;
using PhiBook.Web.Queries;
using Raven.Client;

namespace PhiBook.Web.Modules
{
    public abstract class BaseModule : NancyModule
    {
        protected BaseModule()
        {
        }

        protected BaseModule(string modulePath) : base(modulePath)
        {
        }
    }
}