using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

public sealed class Auth0Authenticator
{
    private readonly string _auth0domain;
    private readonly string _audience;
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _manager;
    private readonly JwtSecurityTokenHandler _handler;

    public Auth0Authenticator(string auth0Domain, string audience)
    {
        _auth0domain = auth0Domain;
        _audience = audience;
        _manager = new ConfigurationManager<OpenIdConnectConfiguration>($"{auth0Domain}.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
        _handler = new JwtSecurityTokenHandler();
    }

    public async Task<ClaimsPrincipal> AuthenticateAsync(string token, CancellationToken cancellationToken = new CancellationToken())
    {
        // Note: ConfigurationManager<T> has an automatic refresh interval of 1 day.
        //   The config is cached in-between refreshes, so this call actually completes synchronously unless it needs to refresh.
        var config = await _manager.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var validationParameters = new TokenValidationParameters
        {
            ValidIssuer = _auth0domain,
            ValidAudience = _audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = config.SigningKeys,
        };
        var user = _handler.ValidateToken(token, validationParameters, out var _);
        return user;
    }
}

public static class Auth0AuthenticatorExtensions
{
    public static async Task<ClaimsPrincipal> AuthenticateAsync(this Auth0Authenticator @this, AuthenticationHeaderValue header,
        CancellationToken cancellationToken = new CancellationToken())
    {
        if (header == null || !string.Equals(header.Scheme, "Bearer", StringComparison.InvariantCultureIgnoreCase))
            throw new InvalidOperationException("Authentication header does not use Bearer token.");
        return await @this.AuthenticateAsync(header.Parameter, cancellationToken);
    }

    public static Task<ClaimsPrincipal> AuthenticateAsync(this Auth0Authenticator @this, HttpRequestMessage request,
        CancellationToken cancellationToken = new CancellationToken()) =>
        @this.AuthenticateAsync(request.Headers.Authorization, cancellationToken);
}
