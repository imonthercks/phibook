using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using Nancy.Extensions;
using Nancy.Security;
using PhiBook.Web.Queries;

namespace PhiBook.Web.Modules
{
    public class BaseModule : NancyModule
    {
        public BaseModule()
        {
        }

        public BaseModule(string modulePath) : base(modulePath)
        {
        }

        public TResult Query<TResult>(Query<TResult> query)
        {
            return query.Execute();
        }

    }
}