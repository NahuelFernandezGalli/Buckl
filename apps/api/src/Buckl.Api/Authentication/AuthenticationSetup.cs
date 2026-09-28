using Buckl.Application.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Buckl.Api.Authentication;

public static class AuthenticationSetup
{
    /// <summary>The longest subject accepted. Auth0 subjects are far shorter; the cap only keeps a
    /// malformed token from reaching the database.</summary>
    public const int MaxSubjectLength = 255;

    /// <summary>Every endpoint requires a valid Auth0 access token unless it opts out with
    /// <c>AllowAnonymous</c> (ADR-0028). The tenant and audience come from configuration and are
    /// validated when the host starts.</summary>
    public static IServiceCollection AddBucklAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<Auth0Options>()
            .Bind(configuration.GetSection(Auth0Options.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<Auth0Options>>((options, auth0) => ConfigureJwtBearer(options, auth0.Value));

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        services.AddScoped<RequestUser>();
        services.AddScoped<ICurrentUser>(provider => provider.GetRequiredService<RequestUser>());

        return services;
    }

    private static void ConfigureJwtBearer(JwtBearerOptions options, Auth0Options auth0)
    {
        options.Authority = auth0.Issuer;
        options.Audience = auth0.Audience;

        // Keep "sub" as "sub" instead of the long WS-Federation claim type.
        options.MapInboundClaims = false;

        options.TokenValidationParameters.ValidIssuer = auth0.Issuer;
        options.TokenValidationParameters.ValidAudience = auth0.Audience;
        options.TokenValidationParameters.ValidAlgorithms = [SecurityAlgorithms.RsaSha256];
        options.TokenValidationParameters.NameClaimType = BucklClaims.Subject;

        options.Events = new JwtBearerEvents { OnTokenValidated = RequireUsableSubject };
    }

    /// <summary>A signed token without a subject cannot be mapped to a user, so it is rejected as
    /// unauthenticated instead of reaching the transaction filter.</summary>
    private static Task RequireUsableSubject(TokenValidatedContext context)
    {
        var subject = context.Principal?.FindFirst(BucklClaims.Subject)?.Value;

        if (string.IsNullOrWhiteSpace(subject) || subject.Length > MaxSubjectLength)
        {
            context.Fail("The access token has no usable subject.");
        }

        return Task.CompletedTask;
    }
}
