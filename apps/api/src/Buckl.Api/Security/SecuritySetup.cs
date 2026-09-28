using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Buckl.Api.Security;

public static class SecuritySetup
{
    /// <summary>CORS for the web app's origins only, and HSTS for a year. The web app sends its
    /// token in the <c>Authorization</c> header, never in a cookie, so credentials stay off.</summary>
    public static IServiceCollection AddBucklSecurity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<WebAppCorsOptions>()
            .Bind(configuration.GetSection(WebAppCorsOptions.SectionName))
            .Validate(
                options => options.AllowedOrigins.All(WebAppCorsOptions.IsOrigin),
                "Every Cors:AllowedOrigins entry must be an origin such as https://buckl.app: scheme and host, "
                + "optional port, no path or trailing slash.")
            .ValidateOnStart();

        services.AddCors();
        services.AddOptions<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>()
            .Configure<IOptions<WebAppCorsOptions>>((cors, webApp) => cors.AddDefaultPolicy(policy => policy
                .WithOrigins([.. webApp.Value.AllowedOrigins])
                .WithMethods(HttpMethods.Get, HttpMethods.Post, HttpMethods.Patch)
                .WithHeaders(HeaderNames.Authorization, HeaderNames.ContentType)
                .WithExposedHeaders(HeaderNames.Location)
                .SetPreflightMaxAge(TimeSpan.FromMinutes(10))));

        services.AddHsts(options => options.MaxAge = TimeSpan.FromDays(365));

        return services;
    }

    /// <summary>Headers for a JSON API that is never framed, never sniffed and never cached. Added
    /// when the response starts, so error responses carry them too. The development API reference
    /// is an HTML page that loads scripts, so it is left without the content security policy.</summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                headers.XContentTypeOptions = "nosniff";
                headers.XFrameOptions = "DENY";
                headers["Referrer-Policy"] = "no-referrer";

                if (!context.Request.Path.StartsWithSegments("/scalar", StringComparison.OrdinalIgnoreCase))
                {
                    headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
                }

                if (string.IsNullOrEmpty(headers.CacheControl))
                {
                    headers.CacheControl = "no-store";
                }

                return Task.CompletedTask;
            });

            await next(context);
        });
}
