using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using Nancy.Responses;

namespace PhiBook.Web
{
    public static class AppHarborSecurityExtensions
    {
        /// <summary>
        /// This module requires https.
        /// </summary>
        /// <param name="module">The <see cref="NancyModule"/> that requires HTTPS.</param>
        public static void RequiresXForwardProtoHeader(this NancyModule module)
        {
            module.RequiresXForwardProtoHeader(true);
        }

        /// <summary>
        /// This module requires https.
        /// </summary>
        /// <param name="module">The <see cref="NancyModule"/> that requires HTTPS.</param>
        /// <param name="redirect"><see langword="true"/> if the user should be redirected to HTTPS if the incoming request was made using HTTP, otherwise <see langword="false"/> if <see cref="HttpStatusCode.Forbidden"/> should be returned.</param>
        public static void RequiresXForwardProtoHeader(this NancyModule module, bool redirect)
        {
            module.Before.AddItemToEndOfPipeline(RequiresHttps(redirect));
        }

        private static Func<NancyContext, Response> RequiresHttps(bool redirect)
        {
            return (ctx) =>
            {
                Response response = null;
                var request = ctx.Request;

                if (ctx.Request.Headers["X-Forwarded-Proto"].FirstOrDefault(x => x == "https") == null)
                {
                    if (redirect)
                    {
                        var redirectUrl = request.Url.Clone();
                        redirectUrl.Scheme = "https";
                        response = new RedirectResponse(redirectUrl.ToString());
                    }
                    else
                    {
                        response = new Response {StatusCode = HttpStatusCode.Forbidden};
                    }
                }
                return response;
            };
        }
    }
}