using System.Net;
using Buckl.Application.Photos;
using Buckl.Domain.Garments;
using Buckl.Domain.Users;
using Buckl.Testing;

namespace Buckl.Api.Tests.Photos;

/// <summary>The whole photo flow as the web app drives it: ask for a ticket, upload (played by
/// <see cref="InMemoryPhotoStorage.Put"/>), then create or edit the garment with the upload
/// id.</summary>
public class GarmentPhotoEndpointsTests
{
    private const string Shirt = """{ "category": "top", "color": "blue", "size": "M" }""";

    private readonly BucklApiFactory _api;

    public GarmentPhotoEndpointsTests(BucklApiFactory api)
    {
        _api = api;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Adding_a_garment_with_an_uploaded_photo_keeps_it_under_the_owners_photos()
    {
        var (subject, owner) = await ProvisionAsync();
        using var client = _api.CreateClientFor(subject);
        var uploadId = await UploadAsync(client, owner);

        using var response = await client.PostAsync(
            Http.Url("/garments"),
            Http.Json($$"""{ "classification": {{Shirt}}, "photo": { "uploadId": "{{uploadId}}" } }"""),
            Ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.ReadJsonAsync(Ct);
        var id = new GarmentId(body.GetProperty("id").GetGuid());
        var key = $"users/{owner.Value:D}/garments/{uploadId:N}.jpg";
        Assert.Equal(
            InMemoryPhotoStorage.ReadUrlFor(key, TestClock.Now.AddHours(1)).AbsoluteUri,
            body.GetProperty("photoUrl").GetString());
        Assert.Equal(key, await _api.Database.ReadGarmentPhotoKeyAsync(id, Ct));
        Assert.True(_api.Photos.Contains(key));
        Assert.False(_api.Photos.Contains(PhotoUploads.StagingKey(owner, uploadId)));
    }

    [Fact]
    public async Task A_photo_that_was_never_uploaded_is_a_bad_request_and_adds_nothing()
    {
        using var client = _api.CreateClientFor(Subjects.New());

        using var response = await client.PostAsync(
            Http.Url("/garments"),
            Http.Json($$"""{ "classification": {{Shirt}}, "photo": { "uploadId": "{{Guid.NewGuid()}}" } }"""),
            Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("photo.upload_not_found", await response.ReadProblemCodeAsync(Ct));
        using var wardrobe = await client.GetAsync(Http.Url("/garments"), Ct);
        Assert.Equal(0, (await wardrobe.ReadJsonAsync(Ct)).GetArrayLength());
    }

    [Fact]
    public async Task An_upload_that_grew_past_the_limit_is_rejected_and_discarded()
    {
        var (subject, owner) = await ProvisionAsync();
        using var client = _api.CreateClientFor(subject);
        var uploadId = await UploadAsync(client, owner, storedSize: PhotoFile.MaxBytes + 1);

        using var response = await client.PostAsync(
            Http.Url("/garments"),
            Http.Json($$"""{ "classification": {{Shirt}}, "photo": { "uploadId": "{{uploadId}}" } }"""),
            Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("photo.too_large", await response.ReadProblemCodeAsync(Ct));
        Assert.False(_api.Photos.Contains(PhotoUploads.StagingKey(owner, uploadId)));
    }

    [Fact]
    public async Task Another_users_upload_cannot_be_attached()
    {
        var (bobSubject, bobId) = await ProvisionAsync();
        using var bob = _api.CreateClientFor(bobSubject);
        var bobsUpload = await UploadAsync(bob, bobId);
        using var alice = _api.CreateClientFor(Subjects.New());

        using var response = await alice.PostAsync(
            Http.Url("/garments"),
            Http.Json($$"""{ "classification": {{Shirt}}, "photo": { "uploadId": "{{bobsUpload}}" } }"""),
            Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("photo.upload_not_found", await response.ReadProblemCodeAsync(Ct));
        Assert.True(_api.Photos.Contains(PhotoUploads.StagingKey(bobId, bobsUpload)));
    }

    [Fact]
    public async Task A_photo_without_its_upload_id_is_malformed()
    {
        using var client = _api.CreateClientFor(Subjects.New());

        using var response = await client.PostAsync(
            Http.Url("/garments"),
            Http.Json($$"""{ "classification": {{Shirt}}, "photo": {} }"""),
            Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("request.invalid", await response.ReadProblemCodeAsync(Ct));
    }

    [Fact]
    public async Task Patching_a_photo_without_its_upload_id_is_malformed()
    {
        var (subject, owner) = await ProvisionAsync();
        using var client = _api.CreateClientFor(subject);
        var (id, key) = await AddWithPhotoAsync(client, owner);

        using var response = await client.PatchAsync(Http.Url($"/garments/{id.Value}"), Http.Json("""{ "photo": {} }"""), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("request.invalid", await response.ReadProblemCodeAsync(Ct));
        Assert.Equal(key, await _api.Database.ReadGarmentPhotoKeyAsync(id, Ct));
    }

    [Fact]
    public async Task Patching_the_photo_replaces_it_and_deletes_the_old_one()
    {
        var (subject, owner) = await ProvisionAsync();
        using var client = _api.CreateClientFor(subject);
        var (id, oldKey) = await AddWithPhotoAsync(client, owner);
        var newUpload = await UploadAsync(client, owner);

        using var response = await client.PatchAsync(
            Http.Url($"/garments/{id.Value}"),
            Http.Json($$"""{ "photo": { "uploadId": "{{newUpload}}" } }"""),
            Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"users/{owner.Value:D}/garments/{newUpload:N}.jpg", await _api.Database.ReadGarmentPhotoKeyAsync(id, Ct));
        Assert.False(_api.Photos.Contains(oldKey));
    }

    [Fact]
    public async Task Patching_the_photo_to_null_removes_and_deletes_it()
    {
        var (subject, owner) = await ProvisionAsync();
        using var client = _api.CreateClientFor(subject);
        var (id, oldKey) = await AddWithPhotoAsync(client, owner);

        using var response = await client.PatchAsync(Http.Url($"/garments/{id.Value}"), Http.Json("""{ "photo": null }"""), Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(await _api.Database.ReadGarmentPhotoKeyAsync(id, Ct));
        Assert.False(_api.Photos.Contains(oldKey));
    }

    [Fact]
    public async Task Patching_other_fields_keeps_the_photo()
    {
        var (subject, owner) = await ProvisionAsync();
        using var client = _api.CreateClientFor(subject);
        var (id, key) = await AddWithPhotoAsync(client, owner);

        using var response = await client.PatchAsync(Http.Url($"/garments/{id.Value}"), Http.Json("""{ "notes": "Linen" }"""), Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(key, await _api.Database.ReadGarmentPhotoKeyAsync(id, Ct));
        Assert.True(_api.Photos.Contains(key));
    }

    [Fact]
    public async Task An_archived_garment_leaves_the_upload_untouched()
    {
        var (subject, owner) = await ProvisionAsync();
        using var client = _api.CreateClientFor(subject);
        var (id, _) = await AddWithPhotoAsync(client, owner);
        using var archived = await client.PostAsync(Http.Url($"/garments/{id.Value}/archive"), content: null, Ct);
        var upload = await UploadAsync(client, owner);

        using var response = await client.PatchAsync(
            Http.Url($"/garments/{id.Value}"),
            Http.Json($$"""{ "photo": { "uploadId": "{{upload}}" } }"""),
            Ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.True(_api.Photos.Contains(PhotoUploads.StagingKey(owner, upload)));
    }

    private async Task<(string Subject, UserId Owner)> ProvisionAsync()
    {
        var subject = Subjects.New();

        return (subject, await _api.ProvisionAsync(subject, Ct));
    }

    /// <summary>Asks for a ticket like the web app, then plays the browser's PUT.</summary>
    private async Task<Guid> UploadAsync(HttpClient client, UserId owner, long storedSize = 150_000)
    {
        using var response = await client.PostAsync(
            Http.Url("/photos/uploads"),
            Http.Json("""{ "contentType": "image/jpeg", "size": 150000 }"""),
            Ct);
        var uploadId = (await response.ReadJsonAsync(Ct)).GetProperty("uploadId").GetGuid();
        _api.Photos.Put(PhotoUploads.StagingKey(owner, uploadId), storedSize, "image/jpeg");

        return uploadId;
    }

    private async Task<(GarmentId Id, string Key)> AddWithPhotoAsync(HttpClient client, UserId owner)
    {
        var uploadId = await UploadAsync(client, owner);
        using var response = await client.PostAsync(
            Http.Url("/garments"),
            Http.Json($$"""{ "classification": {{Shirt}}, "photo": { "uploadId": "{{uploadId}}" } }"""),
            Ct);
        var id = new GarmentId((await response.ReadJsonAsync(Ct)).GetProperty("id").GetGuid());

        return (id, $"users/{owner.Value:D}/garments/{uploadId:N}.jpg");
    }
}
