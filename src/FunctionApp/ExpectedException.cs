using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp
{
    public sealed class ExpectedException : Exception
    {
        public ExpectedException(HttpStatusCode code, string message = "")
            : base(message)
        {
            Code = code;
        }

        public HttpStatusCode Code { get; }
    }
}
