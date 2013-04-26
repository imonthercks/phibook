using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.Security;
using PhiBook.Web.Queries;
using Raven.Client;

namespace PhiBook.Web.Modules
{
    public abstract class RavenModule : BaseModule
    {
        public IDocumentSession DocumentSession
        {
            get { return Context.Items["RavenSession"] as IDocumentSession; }
        }

        protected RavenModule(string modulePath) : base(modulePath)
        {
#if !DEBUG
            this.RequiresXForwardProtoHeader(true);
#endif
            this.RequiresAuthentication();
        }

        public TResult Query<TResult>(Query<TResult> query)
        {
            query.DocumentSession = DocumentSession;
            return query.Execute();
        }

    }
}