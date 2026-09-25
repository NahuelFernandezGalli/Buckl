namespace Buckl.Api.Tests.OpenApi;

public class OpenApiDocumentTests
{
    private readonly BucklApiFactory _api;

    public OpenApiDocumentTests(BucklApiFactory api)
    {
        _api = api;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task The_document_declares_the_bearer_token_every_operation_needs()
    {
        using var client = _api.CreateClient();

        using var response = await client.GetAsync(Http.Url("/openapi/v1.json"), Ct);
        var document = await response.ReadJsonAsync(Ct);

        var scheme = document.GetProperty("components").GetProperty("securitySchemes").GetProperty("Bearer");
        Assert.Equal("http", scheme.GetProperty("type").GetString());
        Assert.Equal("bearer", scheme.GetProperty("scheme").GetString());
        Assert.Equal("JWT", scheme.GetProperty("bearerFormat").GetString());
        var requirement = Assert.Single(document.GetProperty("security").EnumerateArray());
        Assert.True(requirement.TryGetProperty("Bearer", out _));
    }
}
