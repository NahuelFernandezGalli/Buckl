using System.Net;
using Buckl.Testing;

namespace Buckl.Api.Tests.Photos;

public class PhotoUploadEndpointsTests
{
    private readonly BucklApiFactory _api;

    public PhotoUploadEndpointsTests(BucklApiFactory api)
    {
        _api = api;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Asking_to_upload_a_jpeg_returns_a_put_signed_for_the_callers_staging_area()
    {
        var subject = Subjects.New();
        var owner = await _api.ProvisionAsync(subject, Ct);
        using var client = _api.CreateClientFor(subject);

        using var response = await client.PostAsync(
            Http.Url("/photos/uploads"),
            Http.Json("""{ "contentType": "image/jpeg", "size": 184320 }"""),
            Ct);
        var ticket = await response.ReadJsonAsync(Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var uploadId = ticket.GetProperty("uploadId").GetGuid();
        var expiresAt = TestClock.Now.AddMinutes(10);
        var key = $"uploads/{owner.Value:D}/{uploadId:D}";
        // Compared as URIs: System.Text.Json writes a Uri's original string, not its escaped form.
        Assert.Equal(InMemoryPhotoStorage.UploadUrlFor(key, "image/jpeg", expiresAt), new Uri(ticket.GetProperty("url").GetString()!));
        Assert.Equal("PUT", ticket.GetProperty("method").GetString());
        Assert.Equal("image/jpeg", ticket.GetProperty("headers").GetProperty("Content-Type").GetString());
        Assert.Equal(expiresAt, ticket.GetProperty("expiresAt").GetDateTimeOffset());
    }

    [Theory]
    [InlineData("image/gif", 1000L, "photo.unsupported_type")]
    [InlineData("image/jpeg", 0L, "photo.empty")]
    [InlineData("image/jpeg", 5_242_881L, "photo.too_large")]
    public async Task A_photo_the_rules_reject_is_a_bad_request_with_its_code(string contentType, long size, string code)
    {
        using var client = _api.CreateClientFor(Subjects.New());

        using var response = await client.PostAsync(
            Http.Url("/photos/uploads"),
            Http.Json($$"""{ "contentType": "{{contentType}}", "size": {{size}} }"""),
            Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(code, await response.ReadProblemCodeAsync(Ct));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{ "contentType": "image/jpeg" }""")]
    [InlineData("""{ "size": 10 }""")]
    public async Task A_request_without_type_or_size_is_malformed(string json)
    {
        using var client = _api.CreateClientFor(Subjects.New());

        using var response = await client.PostAsync(Http.Url("/photos/uploads"), Http.Json(json), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("request.invalid", await response.ReadProblemCodeAsync(Ct));
    }

    [Fact]
    public async Task Only_a_signed_in_user_can_ask_to_upload()
    {
        using var client = _api.CreateClient();

        using var response = await client.PostAsync(
            Http.Url("/photos/uploads"),
            Http.Json("""{ "contentType": "image/jpeg", "size": 10 }"""),
            Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
