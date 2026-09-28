using Buckl.Api.Tests.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Buckl.Api.Tests;

/// <summary>The API in the Production environment, trusting the test tenant, without a database:
/// for what depends on the environment and on configuration read at startup. Later settings win,
/// so a test overrides only the value it is about.</summary>
internal static class ProductionHost
{
    public static WebApplicationFactory<Program> Create(params (string Key, string Value)[] settings) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder
                .UseEnvironment(Environments.Production)
                .UseSetting("Auth0:Domain", TestTokens.Domain)
                .UseSetting("Auth0:Audience", TestTokens.Audience);

            foreach (var (key, value) in settings)
            {
                builder.UseSetting(key, value);
            }
        });
}
