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
    public class ExpectedException : Exception
    {
        public ExpectedException(HttpStatusCode code, string message = "")
            : base(message)
        {
            Code = code;
        }

        public HttpStatusCode Code { get; }

        public HttpResponseMessage CreateErroResponseMessage(HttpRequestMessage request)
        {
            var result = request.CreateErrorResponse(Code, Message);
            ApplyResponseDetails(result);
            return result;
        }

        protected virtual void ApplyResponseDetails(HttpResponseMessage response) { }
    }
}
