using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json.Linq;

namespace FunctionApp
{
    public static class AdvancedFunction
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        [FunctionName("FunctionAdvanced")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "advanced")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                log.Info("C# HTTP trigger function processed a request.");

                // The "user" returned here is an actual ClaimsPrincipal with the claims that were in the access_token.
                // The "token" is a SecurityToken that can be used to invoke services on the part of the user. E.g., create a Google Calendar event on the user's calendar.
                var (user, token) = await req.AuthenticateAsync(log);

                // Dump the claims details in the user
                log.Info("User authenticated");
                foreach (var claim in user.Claims)
                    log.Info($"Claim `{claim.Type}` is `{claim.Value}`");

                // Hit the auth0 user_info API and see what we get back about this user
                var userinfo = await Auth0Userinfo(req.Headers.Authorization);

                // Return the user details to the calling app.
                var results = new
                {
                    userinfo,
                    claims = user.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList(),
                };  
                return req.CreateResponse(HttpStatusCode.OK, results);
            }
            catch (ExpectedException ex)
            {
                return req.CreateErrorResponse(ex.Code, ex.Message);
            }
        }

        public static async Task<JObject> Auth0Userinfo(AuthenticationHeaderValue token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://{Constants.Auth0Domain}/userinfo");
            request.Headers.Authorization = token;
            var result = await HttpClient.SendAsync(request);
            return await result.Content.ReadAsAsync<JObject>();
        }
    }
}
