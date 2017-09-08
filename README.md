# Azure Functions + Auth0

Using Auth0 to authenticate an HTTP-triggered C# class library Azure Function

# How It Works (Client Side)

[Follow along!](https://stephenclearyexamples.github.io/FunctionsAuth0/)

The single HTML page under `docs/index.html` acts like a SPA. A super-simple one.

When it first loads, it checks the hash fragment of the URL. There isn't any, so it knows the user is not logged in.

> Note: Real-world SPAs will store their user tokens in local storage, and check them to see if the user is logged in. This super-simple SPA deliberately does not, so every time you refresh the page, you'll be logged out. It's a feature. ;)

Click the "Call API" button. The SPA will send a request without the `Authorization` header to the Azure Function. The Function will reject the call as unauthorized (`403`).

Click the "Login" button. The SPA will redirect to an Auth0-hosted login page, where you can authorize using social media accounts.

> Note: There are lots of options on how to log someone in via Auth0. It doesn't have to be an Auth0-hosted login page. In particular, SPAs may choose to use [Lock](https://auth0.com/docs/libraries/lock) instead.

The Auth0 login redirects back to the SPA, appending user authentication information in the URL hash.

The SPA loads and sees the authentication information in the URL hash. The `parseHash` call extracts both tokens (`access_token` and `id_token`) from the hash, validates the `id_token`, and knows the user is logged in.

The SPA then parses the `id_token` and displays the user information available to the SPA.

Click the "Call API" button. The SPA will send a request with the `access_token` in the `Authorization` header.

The Azure Function will authorize the user and return the details of the claims it can see in its token. The `access_token` is usually minimal, containing only enough information to identify the user and make requests on behalf of them.

# How It Works (Server Side)

The server code is kicked off by a single line:

    var (user, token) = await req.AuthenticateAsync(log);
    
This behaves as follows:

1. The `Authorization: Bearer {access_token}` header is parsed and the `access_token` is validated.
1. A `SecurityToken` is created by parsing the `access_token`. This is what is returned as `token` in the code above, and can be used to authenticate actions against other APIs (e.g., creating a Google Calendar event for a user).
1. A `ClaimsPrincipal` is created with a single `ClaimsIdentity` that contains all the claims from that `access_token`.
1. The resulting `ClaimsPrincipal` (containing one identity for the `access_token`) and the `SecurityToken` (the parsed `access_token`) are returned.

You can then examine the claims and take action accordingly. This sample Azure Function just returns the claims as JSON.

If there are any authentication errors at all, an exception is raised (and logged), and the Azure Function returns a `403`.

# More Info

The SPA and Azure Function authentication flow follows https://auth0.com/docs/api-auth/grant/implicit

The SPA uses a simplified version of https://auth0.com/docs/quickstart/spa/jquery (in this simple example, there is no saving of tokens, no logout, no profile or scope handling, and no token renewal).

The Azure Functions side was a bit harder, since the Auth0 docs assume you're running on ASP.NET (with the capability to configure the OWIN authentication middleware). Since that's not true in the Azure Functions world, I had to take cues from the [general Auth0 API docs](https://auth0.com/docs/api-auth/tutorials/verify-access-token). The [manually validate JWT on .NET example](https://github.com/auth0-samples/auth0-dotnet-validate-jwt/tree/master/IdentityModel-RS256) was much more helpful. Finally, I used the [Auth0 Python API quickstart](https://auth0.com/docs/quickstart/backend/python) to determine the correct handling of the `Authorization` header.

# How to Set It Up with Your Own Accounts

There's a number of settings that need to be coordinated to get authorization working with your own accounts.

1) Fork this GitHub project.
1) Enable GitHub Pages under the project settings (as `master branch /docs folder`).
   - this will act as your SPA.
1) Create an Azure Functions app.
1) [Set up deployment](https://docs.microsoft.com/en-us/azure/azure-functions/functions-continuous-deployment) from your GitHub project.
1) Create an Auth0 client (Single Page Web Application).
   1) In the `Allowed Callback URLs` setting, add your GH Pages URL (under your GitHub repository settings).
      - this setting tells Auth0 that after logging a user in, they are allowed to return to your SPA.
1) In your Azure Functions app settings:
   1) Set `AUTH0_DOMAIN` to your Auth0 Domain (under your Auth0 client settings).
      - this setting tells the Function App authentication code which Auth0 account to use.
   1) Set `AUTH0_AUDIENCE` to your Azure Functions URL (in the Azure Functions overview).
      - this setting tells the Function App authentication code that it is the target audience for the `access_token`.
1) In your Azure Functions CORS settings, add the domain portion of your GH Pages URL (under your GitHub repository settings).
   - this setting tells your Function App that it should receive requests from the SPA.
1) Create an Auth0 API, with `Identifier` set to your Azure Functions URL (in the Azure Functions overview).
   - this setting tells Auth0 that our Azure Functions App is the target audience for the `access_token`.
1) Update `docs/index.html`:
   1) Change `AUTH0_DOMAIN` to your Auth0 Domain (under your Auth0 client settings).
      - this setting tells the SPA which Auth0 account to use.
   1) Change `AUTH0_CLIENT_ID` to your Auth0 Client ID (under your Auth0 client settings).
      - this setting tells the SPA that it is the target audience for the `id_token`.
   1) Change `AUTH0_CALLBACK_URL` to your GH Pages URL (under your GitHub repository settings).
      - this setting is used by the SPA to tell Auth0 where to go after logging in a user.
   1) Change `FUNCTIONS_URL` to your Azure Functions URL (in the Azure Functions overview).
      - this setting tells the SPA that the target audience for the `access_token` is the Azure Functions app
      - it's also used as the base URL when calling the Azure Function
