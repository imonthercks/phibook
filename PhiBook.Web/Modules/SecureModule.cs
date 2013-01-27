using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.Security;

namespace PhiBook.Web.Modules
{
    public class SecureModule : BaseModule
    {
        public SecureModule()
        {
#if !DEBUG
            this.RequiresHttps(true);
#endif
            this.RequiresAuthentication();
        }

        public SecureModule(string modulePath)
            : base(modulePath)
        {
#if !DEBUG
            this.RequiresHttps(true);
#endif
            this.RequiresAuthentication();
        }

    }
}