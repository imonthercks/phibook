//using System;
//using System.Linq;
//using System.Web;
//using Nancy.Security;
//using PhiBook.Web.Models.Auth;

//namespace PhiBook.Web.Queries
//{
//    public class IdentityByApiKey : Query<IUserIdentity>
//    {
//        private readonly string _apiKey;

//        public IdentityByApiKey(string apiKey)
//        {
//            _apiKey = apiKey;
//        }

//        public override IUserIdentity Execute()
//        {
//            var user = AuthQuery.Users.FirstOrDefault(x => x.Value.ApiKey == _apiKey);

//            return new PhiBookIdentity {UserName = user.Value.UserId};
//        }
//    }
//}