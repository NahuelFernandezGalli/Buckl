using System.Security.Cryptography;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Buckl.Api.Tests.Authentication;

/// <summary>A stand-in Auth0 tenant: it signs access tokens with a key generated per test run, and
/// <see cref="Configuration"/> is the metadata the API would download from the real tenant. The
/// API validates these tokens with exactly the code it runs in production.</summary>
internal static class TestTokens
{
    public const string Domain = "buckl-tests.local";

    public const string Issuer = $"https://{Domain}/";

    public const string Audience = "https://api.buckl.test";

    public static readonly RsaSecurityKey SigningKey = NewKey("buckl-tests");

    /// <summary>Signs tokens the API must reject: same algorithm, different key.</summary>
    public static readonly RsaSecurityKey ForeignKey = NewKey("somebody-else");

    public static OpenIdConnectConfiguration Configuration { get; } = CreateConfiguration();

    public static string For(string subject) => Issue(new TestToken { Subject = subject });

    public static string Issue(TestToken token)
    {
        ArgumentNullException.ThrowIfNull(token);

        var claims = new Dictionary<string, object>();

        if (token.Subject is not null)
        {
            claims[JwtRegisteredClaimNames.Sub] = token.Subject;
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = token.Issuer,
            Audience = token.Audience,
            Claims = claims,
            IssuedAt = token.NotBefore,
            NotBefore = token.NotBefore,
            Expires = token.Expires,
            SigningCredentials = token.Key is null
                ? null
                : new SigningCredentials(token.Key, token.Algorithm),
        };

        return new JsonWebTokenHandler { SetDefaultTimesOnTokenCreation = false }.CreateToken(descriptor);
    }

    private static RsaSecurityKey NewKey(string keyId) => new(RSA.Create(2048)) { KeyId = keyId };

    private static OpenIdConnectConfiguration CreateConfiguration()
    {
        var configuration = new OpenIdConnectConfiguration { Issuer = Issuer };
        configuration.SigningKeys.Add(SigningKey);

        return configuration;
    }
}

/// <summary>A valid token for a fresh subject unless a test changes one property.</summary>
internal sealed record TestToken
{
    public string? Subject { get; init; } = Subjects.New();

    public string Issuer { get; init; } = TestTokens.Issuer;

    public string Audience { get; init; } = TestTokens.Audience;

    public DateTime NotBefore { get; init; } = DateTime.UtcNow.AddMinutes(-1);

    public DateTime Expires { get; init; } = DateTime.UtcNow.AddMinutes(5);

    /// <summary><c>null</c> issues an unsigned token (<c>alg: none</c>).</summary>
    public SecurityKey? Key { get; init; } = TestTokens.SigningKey;

    public string Algorithm { get; init; } = SecurityAlgorithms.RsaSha256;
}
