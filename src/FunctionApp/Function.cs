using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Threading.Tasks;

namespace FunctionApp
{
    public static class Function
    {
        [FunctionName("FunctionHello")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "hello")]HttpRequestMessage req, TraceWriter log)
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

                // Return the user details to the calling app.
                var result = user.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList();
                return req.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (ExpectedException ex)
            {
                return req.CreateErrorResponse(ex.Code, ex.Message);
            }
        }
    }
}
