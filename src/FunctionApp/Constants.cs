using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp
{
    public static class Constants
    {
        public static string Auth0Domain => GetEnvironmentVariable("AUTH0_DOMAIN");
        public static string Audience => GetEnvironmentVariable("AUTH0_AUDIENCE");
        public static string ClientId => GetEnvironmentVariable("AUTH0_CLIENT_ID");

        private static string GetEnvironmentVariable(string name)
        {
            var result = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(result))
                throw new InvalidOperationException($"Missing app setting {name}");
            return result;
        }
    }
}
