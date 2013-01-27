using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhiBook.Web.Models.Auth
{
    public class AuthUser
    {
        public string Id { get; set; }
        public string ApiKey { get; set; }
        public string UserId { get; set; }
        public string HashedPassword { get; set; }
    }
}