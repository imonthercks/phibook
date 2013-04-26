using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Raven.Client;

namespace PhiBook.Web.Queries
{
    public abstract class Query<TResult>
    {
        public IDocumentSession DocumentSession { get; set; } 
        public abstract TResult Execute();
    }
}