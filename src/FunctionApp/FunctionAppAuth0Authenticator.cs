using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.IdentityModel.Tokens;

namespace FunctionApp
{
    // This is a singleton, since we want to share the Auth0Authenticator instance across all function app invocations in this appdomain.
    public static class FunctionAppAuth0Authenticator
    {
        // I'm using a Lazy here just so that exceptions on startup are in the scope of a function execution.
        // I'm using PublicationOnly so that exceptions during creation are retried on the next execution.
        private static readonly Lazy<Auth0Authenticator> Authenticator = new Lazy<Auth0Authenticator>(() => new Auth0Authenticator(Constants.Auth0Domain, new [] { Constants.Audience }));

        /// <summary>
        /// Authenticates the user via an "Authentication: Bearer {token}" header in an HTTP request message.
        /// Returns a user principal containing claims from the token(s) and a token that can be used to perform actions on behalf of the user.
        /// Throws an exception if any of the tokens fail to authenticate or if the Authentication header is missing or malformed.
        /// This method has an asynchronous signature, but usually completes synchronously.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="log">A log used to write the authentication failure to.</param>
        public static async Task<(ClaimsPrincipal User, SecurityToken ValidatedToken)> AuthenticateAsync(this HttpRequestMessage request, TraceWriter log)
        {
            var authenticator = Authenticator.Value;
            try
            {
                return await authenticator.AuthenticateAsync(request);
            }
            catch (Exception ex)
            {
                log.Error("Authorization failed", ex);
                throw new AuthenticationExpectedException();
            }
        }
    }
}
