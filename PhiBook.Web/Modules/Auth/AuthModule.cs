﻿using System;
using System.Web;
using Nancy;
using Nancy.Cookies;
using Nancy.Cryptography;
using Nancy.ModelBinding;
using Nancy.Responses;
using PhiBook.Web.Models.Auth;
using PhiBook.Web.ViewModels;
using Raven.Client;

namespace PhiBook.Web.Modules.Auth
{
    public class AuthModule : BaseModule
    {
        public AuthModule(IDocumentSession ravenSession)
            : base("/auth/")
        {
#if !DEBUG
            this.RequiresXForwardProtoHeader(true);
#endif

            Get["/login"] = parameters => View["login.cshtml", (string)Request.Query.url];
            
            //the Post["/login"] method is used mainly to fetch the api key for subsequent calls
            Post["/login"] = x =>
                                 {
                                     var requestContent = this.Bind<AuthCredential>();

                                     var authUser = ravenSession.Load<AuthUser>(requestContent.Username);

                                     if (authUser == null ||
                                         authUser.HashedPassword != HashPassword(requestContent.Password))
                                         return new Response {StatusCode = HttpStatusCode.Unauthorized};

                                     var apiKey = authUser.ApiKey;

                                     var responseUrl = Request.Form.url;

                                     var authCookie = BuildCookie(apiKey, DateTime.Now.AddDays(1));

                                     if (string.IsNullOrEmpty(responseUrl))
                                         return
                                             (new Response {StatusCode = HttpStatusCode.NoContent}).AddCookie(authCookie);

                                     var response =
                                         new RedirectResponse(HttpUtility.HtmlDecode(responseUrl)).AddCookie(authCookie);

                                     return response;
                                 };

            //do something to destroy the api key, maybe?                    
            Delete["/"] = x => new Response {StatusCode = HttpStatusCode.OK};



        }

        private static string HashPassword(string password)
        {
            return password;
        }

        /// <summary>
        /// Build the forms authentication cookie
        /// </summary>
        /// <param name="userIdentifier">Authenticated user identifier</param>
        /// <param name="cookieExpiry">Optional expiry date for the cookie (for 'Remember me')</param>
        /// <returns>Nancy cookie instance</returns>
        private static INancyCookie BuildCookie(string userIdentifier, DateTime? cookieExpiry)
        {
            var cookieContents = EncryptAndSignCookie(userIdentifier);

#if DEBUG
            var cookie = new NancyCookie("_phibook-sa", cookieContents, true, false) { Expires = cookieExpiry };
#else
            var cookie = new NancyCookie("_phibook-sa", cookieContents, true, true) { Expires = cookieExpiry };
#endif

            return cookie;
        }

        /// <summary>
        /// Encrypt and sign the cookie contents
        /// </summary>
        /// <param name="cookieValue">Plain text cookie value</param>
        /// <returns>Encrypted and signed string</returns>
        private static string EncryptAndSignCookie(string cookieValue)
        {
            var encryptedCookie = CryptographyConfiguration.Default.EncryptionProvider.Encrypt(cookieValue);
            var hmacBytes = GenerateHmac(encryptedCookie);
            var hmacString = Convert.ToBase64String(hmacBytes);

            return String.Format("{1}{0}", encryptedCookie, hmacString);
        }

        /// <summary>
        /// Generate a hmac for the encrypted cookie string
        /// </summary>
        /// <param name="encryptedCookie">Encrypted cookie string</param>
        /// <returns>Hmac byte array</returns>
        private static byte[] GenerateHmac(string encryptedCookie)
        {
            return CryptographyConfiguration.Default.HmacProvider.GenerateHmac(encryptedCookie);
        }
    }
}