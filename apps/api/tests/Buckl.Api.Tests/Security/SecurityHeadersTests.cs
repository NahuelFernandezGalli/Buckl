using Microsoft.AspNetCore.Mvc.Testing;

namespace Buckl.Api.Tests.Security;

public class SecurityHeadersTests
{
    private readonly BucklApiFactory _api;

    public SecurityHeadersTests(BucklApiFactory api)
    {
        _api = api;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData("/health")]
    [InlineData("/garments")]
    [InlineData("/nowhere")]
    public async Task Every_response_carries_the_security_headers(string path)
    {
        using var client = _api.CreateClient();

        using var response = await client.GetAsync(Http.Url(path), Ct);

        Assert.Equal("nosniff", Header(response, "X-Content-Type-Options"));
        Assert.Equal("DENY", Header(response, "X-Frame-Options"));
        Assert.Equal("no-referrer", Header(response, "Referrer-Policy"));
        Assert.Equal("default-src 'none'; frame-ancestors 'none'", Header(response, "Content-Security-Policy"));
        Assert.True(response.Headers.CacheControl?.NoStore);
    }

    [Fact]
    public async Task Development_does_not_ask_browsers_for_strict_transport_security()
    {
        using var client = _api.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://api.buckl.test"),
        });

        using var response = await client.GetAsync(Http.Url("/health"), Ct);

        Assert.False(response.Headers.Contains("Strict-Transport-Security"));
    }

    [Fact]
    public async Task Production_asks_browsers_for_strict_transport_security_over_https()
    {
        using var factory = ProductionHost.Create();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://api.buckl.test"),
        });

        using var response = await client.GetAsync(Http.Url("/health"), Ct);

        Assert.Equal("max-age=31536000", Header(response, "Strict-Transport-Security"));
    }

    private static string Header(HttpResponseMessage response, string name) =>
        Assert.Single(response.Headers.TryGetValues(name, out var values) ? values : response.Content.Headers.GetValues(name));
}
