using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Cryptography;
using Nancy.Elmah;
using Nancy.Extensions;
using Nancy.Security;
using PhiBook.Web.Models.Auth;
using PhiBook.Web.Queries;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;

namespace PhiBook.Web
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            base.ApplicationStartup(container, pipelines);
            Elmahlogging.Enable(pipelines, "elmah");
        }

        protected override void ConfigureApplicationContainer(Nancy.TinyIoc.TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            var store = new DocumentStore
                            {
                                ConnectionStringName = "RavenDB"
                            };

            store.Initialize();
            container.Register<IDocumentStore>(store);
        }

        protected override void ConfigureRequestContainer(Nancy.TinyIoc.TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            var docStore = container.Resolve<IDocumentStore>();
            var docSession = docStore.OpenSession();
            container.Register(docSession);
            context.Items.Add("RavenSession", docSession);
        }

        protected override void RequestStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);

            var configuration =
                new StatelessAuthenticationConfiguration(GetLoadAuthenticationHook);

            StatelessAuthentication.Enable(pipelines, configuration);

            pipelines.AfterRequest.AddItemToEndOfPipeline(
                (ctx) =>
                {
                    var session = container.Resolve<IDocumentSession>();

                    if (session == null)
                        return;

                    session.SaveChanges();
                    session.Dispose();
                });

            //pipelines.BeforeRequest.AddItemToStartOfPipeline(GetLoadAuthenticationHook(configuration));

            pipelines.AfterRequest.AddItemToEndOfPipeline(GetRedirectToLoginHook());

        }

        /// <summary>
        /// Gets the pre request hook for loading the authenticated user's details
        /// from the cookie.
        /// </summary>
        /// <returns>Pre request hook delegate</returns>
        private static IUserIdentity GetLoadAuthenticationHook(NancyContext context)
        {
            var apiKey = GetAuthenticatedApiKeyFromCookie(context);

            if (!string.IsNullOrEmpty(apiKey))
            {
                var session = context.Items["RavenSession"] as IDocumentSession;
                if (session == null)
                    return null;

                var user = session.Query<AuthUser>()
                    .FirstOrDefault(x => x.ApiKey == apiKey);

                if (user != null) return new PhiBookIdentity { UserName = user.UserId };
            }

            return null;
        }

        /// <summary>
        /// Gets the post request hook for redirecting to the login page
        /// </summary>
        /// <returns>Post request hook delegate</returns>
        private static Action<NancyContext> GetRedirectToLoginHook()
        {
            return context =>
            {
                if (context.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    context.Response = context.GetRedirect(
                        string.Format("{0}?{1}={2}",
                        context.ToFullPath("~/auth/login"),
                        "url",
                        context.ToFullPath("~" + context.Request.Path + HttpUtility.UrlEncode(context.Request.Url.Query))));
                }
            };
        }

        /// <summary>
        /// Gets the authenticated user GUID from the incoming request cookie if it exists
        /// and is valid.
        /// </summary>
        /// <param name="context">Current context</param>
        /// <returns>Returns user guid, or Guid.Empty if not present or invalid</returns>
        private static string GetAuthenticatedApiKeyFromCookie(NancyContext context)
        {
            if (!context.Request.Cookies.ContainsKey("_phibook-sa"))
            {
                return string.Empty;
            }

            var cookieValue = DecryptAndValidateAuthenticationCookie(context.Request.Cookies["_phibook-sa"]);

            return cookieValue;
        }

        /// <summary>
        /// Decrypt and validate an encrypted and signed cookie value
        /// </summary>
        /// <param name="cookieValue">Encrypted and signed cookie value</param>
        /// <returns>Decrypted value, or empty on error or if failed validation</returns>
        private static string DecryptAndValidateAuthenticationCookie(string cookieValue)
        {
            // TODO - shouldn't this be automatically decoded by nancy cookie when that change is made?
            var decodedCookie = Nancy.Helpers.HttpUtility.UrlDecode(cookieValue);

            var hmacStringLength = Base64Helpers.GetBase64Length(CryptographyConfiguration.Default.HmacProvider.HmacLength);

            var encryptedCookie = decodedCookie.Substring(hmacStringLength);
            var hmacString = decodedCookie.Substring(0, hmacStringLength);

            var encryptionProvider = CryptographyConfiguration.Default.EncryptionProvider;

            // Check the hmacs, but don't early exit if they don't match
            var hmacBytes = Convert.FromBase64String(hmacString);
            var newHmac = CryptographyConfiguration.Default.HmacProvider.GenerateHmac(encryptedCookie);
            var hmacValid = HmacComparer.Compare(newHmac, hmacBytes, CryptographyConfiguration.Default.HmacProvider.HmacLength);

            var decrypted = encryptionProvider.Decrypt(encryptedCookie);

            // Only return the decrypted result if the hmac was ok
            return hmacValid ? decrypted : String.Empty;
        }
    }
}