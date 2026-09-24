using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Buckl.Api.Authentication;

/// <summary>Authenticates a request as whatever subject the <see cref="UserHeader"/> header names.
/// Registered only in the Development environment, until Auth0 replaces it in phase 5
/// (ADR-0026). It proves nothing about the caller and must never run anywhere else.</summary>
public sealed class DevelopmentAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Development";

    public const string UserHeader = "X-Dev-User";

    public const int MaxSubjectLength = 255;

    public DevelopmentAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var subject = Request.Headers[UserHeader].ToString().Trim();

        if (subject.Length == 0)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (subject.Length > MaxSubjectLength)
        {
            return Task.FromResult(AuthenticateResult.Fail(
                $"{UserHeader} cannot exceed {MaxSubjectLength} characters."));
        }

        var identity = new ClaimsIdentity([new Claim(BucklClaims.Subject, subject)], SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
