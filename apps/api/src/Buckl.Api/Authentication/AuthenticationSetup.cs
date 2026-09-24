using Buckl.Application.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Buckl.Api.Authentication;

public static class AuthenticationSetup
{
    /// <summary>Every endpoint requires an authenticated user unless it opts out with
    /// <c>AllowAnonymous</c>. Until phase 5 the only scheme is the development one, so the API
    /// refuses to start in any other environment.</summary>
    public static IServiceCollection AddBucklAuthentication(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);

        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Buckl has no production authentication until phase 5 (Auth0). "
                + "Run the API with ASPNETCORE_ENVIRONMENT=Development.");
        }

        services
            .AddAuthentication(DevelopmentAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, DevelopmentAuthenticationHandler>(
                DevelopmentAuthenticationHandler.SchemeName,
                configureOptions: null);

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        services.AddScoped<RequestUser>();
        services.AddScoped<ICurrentUser>(provider => provider.GetRequiredService<RequestUser>());

        return services;
    }
}
