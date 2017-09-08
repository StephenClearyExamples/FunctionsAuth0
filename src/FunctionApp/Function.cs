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
        [FunctionName("Function")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "hello")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                log.Info("C# HTTP trigger function processed a request.");

                var user = await req.AuthenticateAsync(log);

                log.Info("User authenticated as " + user.Identity.Name);

                foreach (var claim in user.Claims)
                    log.Info($"Claim `{claim.Type}` is `{claim.Value}`");

                // Return the user details to the calling app.
                var result = user.Claims.Select(x => new { type = x.Type, value = x.Value }).ToList();

                return req.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (ExpectedException ex)
            {
                return req.CreateErrorResponse(ex.Code, ex.Message);
            }
        }
    }
}
