using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;

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

    }
}