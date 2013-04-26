using System.Collections.Generic;
using Nancy.Security;

namespace PhiBook.Web.Queries
{
    public class PhiBookIdentity : IUserIdentity
    {
        public string UserName { get; set; }
        public IEnumerable<string> Claims { get; set; }
    }
}