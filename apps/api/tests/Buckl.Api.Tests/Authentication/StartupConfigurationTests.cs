using System.Net;
using Microsoft.Extensions.Options;

namespace Buckl.Api.Tests.Authentication;

/// <summary>The API starts in any environment once it knows its Auth0 tenant, and refuses to start
/// without it.</summary>
public class StartupConfigurationTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task The_api_starts_in_production_with_its_auth0_settings()
    {
        using var factory = ProductionHost.Create();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(Http.Url("/health"), Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("Auth0:Domain", "")]
    [InlineData("Auth0:Audience", "")]
    [InlineData("Auth0:Domain", "https://buckl-tests.local/")]
    [InlineData("Auth0:Domain", "buckl-tests.local/")]
    public void The_api_refuses_to_start_without_valid_auth0_settings(string key, string value)
    {
        using var factory = ProductionHost.Create((key, value));

        var exception = Record.Exception(() => factory.CreateClient());

        var invalid = Assert.IsType<OptionsValidationException>(exception);
        Assert.Contains("Auth0", invalid.Message, StringComparison.Ordinal);
    }
}
