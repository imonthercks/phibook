using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;

namespace PhiBook.BatchLoader
{
    public class PhiBookAuthenticator : IAuthenticator
    {
        private readonly string _baseUrl;
        private readonly string _userName;
        private readonly string _password;
        public static IList<RestResponseCookie> AuthCookies = new List<RestResponseCookie>();
     
        public PhiBookAuthenticator(string baseUrl, string userName, string password)
        {
            _baseUrl = baseUrl;
            _userName = userName;
            _password = password;
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            if (AuthCookies.Count == 0)
            {
                var authClient = new RestClient { BaseUrl = _baseUrl };

                var loginRequest = new RestRequest(Method.POST) { Resource = "auth/login", RequestFormat = DataFormat.Json };
                loginRequest.AddBody(new Web.ViewModels.AuthCredential { Username = _userName, Password = _password });
                var loginResponse = authClient.Execute<dynamic>(loginRequest);
                AuthCookies = loginResponse.Cookies;
            }

            foreach (var restResponseCookie in AuthCookies)
            {
                request.AddCookie(restResponseCookie.Name, restResponseCookie.Value);
            }

        }
    }
}
