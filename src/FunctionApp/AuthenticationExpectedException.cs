using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp
{
    public sealed class AuthenticationExpectedException : ExpectedException
    {
        public AuthenticationExpectedException(string message = "")
            : base(HttpStatusCode.Forbidden, message)
        {
        }

        protected override void ApplyResponseDetails(HttpResponseMessage response)
        {
            response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Bearer", "token_type=\"JWT\""));
        }
    }
}
