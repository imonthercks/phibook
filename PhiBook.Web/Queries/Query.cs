using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhiBook.Web.Queries
{
    public abstract class Query<TResult>
    {
        public abstract TResult Execute();
    }
}