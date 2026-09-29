using System.Globalization;
using System.Net.Http.Headers;
using System.Web;
using Buckl.Application.Abstractions;
using Buckl.Infrastructure.Storage;
using Buckl.Testing;
using Microsoft.Extensions.Options;

namespace Buckl.Infrastructure.Tests.Storage;

/// <summary>The adapter against a real S3 implementation: every operation the photo flow needs,
/// through the same SDK calls it makes against B2.</summary>
public sealed class S3PhotoStorageTests : IClassFixture<S3Emulator>, IDisposable
{
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3, 4, 0xFF, 0xD9];

    private readonly S3PhotoStorage _storage;

    private readonly HttpClient _browser = new();

    public S3PhotoStorageTests(S3Emulator emulator)
    {
        _storage = new S3PhotoStorage(Options.Create(emulator.Options));
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static DateTimeOffset InTenMinutes => DateTimeOffset.UtcNow.AddMinutes(10);

    public void Dispose()
    {
        _storage.Dispose();
        _browser.Dispose();
    }

    [Fact]
    public async Task A_photo_uploaded_through_a_signed_url_can_be_found_copied_read_and_deleted()
    {
        var staging = $"uploads/{Guid.NewGuid():D}/{Guid.NewGuid():D}";
        var final = $"users/{Guid.NewGuid():D}/garments/{Guid.NewGuid():N}.jpg";

        using var upload = await PutAsync(_storage.CreateUploadUrl(staging, "image/jpeg", InTenMinutes));
        Assert.True(upload.IsSuccessStatusCode, $"The upload answered {upload.StatusCode}.");

        Assert.Equal(new StoredObject(Jpeg.Length, "image/jpeg"), await _storage.FindAsync(staging, Ct));

        await _storage.CopyAsync(staging, final, Ct);
        await _storage.DeleteAsync(staging, Ct);

        Assert.Null(await _storage.FindAsync(staging, Ct));
        Assert.Equal(Jpeg, await _browser.GetByteArrayAsync(_storage.CreateReadUrl(final, InTenMinutes), Ct));
    }

    [Fact]
    public async Task A_missing_object_is_not_found() =>
        Assert.Null(await _storage.FindAsync($"uploads/{Guid.NewGuid():D}/nothing", Ct));

    [Fact]
    public async Task Deleting_a_missing_object_succeeds() =>
        await _storage.DeleteAsync($"users/{Guid.NewGuid():D}/garments/gone.jpg", Ct);

    [Fact]
    public async Task Copying_a_missing_object_is_a_storage_error() =>
        await Assert.ThrowsAsync<PhotoStorageException>(() => _storage.CopyAsync(
            $"uploads/{Guid.NewGuid():D}/nothing",
            $"users/{Guid.NewGuid():D}/garments/x.jpg",
            Ct));

    [Fact]
    public void An_upload_url_signs_the_content_type_and_carries_no_checksum()
    {
        var query = HttpUtility.ParseQueryString(
            _storage.CreateUploadUrl("uploads/a/b", "image/jpeg", InTenMinutes).Query);

        Assert.Contains("content-type", query["X-Amz-SignedHeaders"], StringComparison.Ordinal);
        // The SDK rounds the remaining lifetime up to whole seconds, so it can read one more.
        Assert.InRange(int.Parse(query["X-Amz-Expires"]!, CultureInfo.InvariantCulture), 590, 601);
        // Not every S3-compatible store accepts the flexible checksums newer SDKs add by default.
        Assert.DoesNotContain(
            query.AllKeys,
            key => key!.StartsWith("x-amz-checksum", StringComparison.OrdinalIgnoreCase)
                || key.Equals("x-amz-sdk-checksum-algorithm", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void A_read_url_expires_when_asked()
    {
        var query = HttpUtility.ParseQueryString(
            _storage.CreateReadUrl("users/a/garments/b.jpg", DateTimeOffset.UtcNow.AddHours(1)).Query);

        Assert.InRange(int.Parse(query["X-Amz-Expires"]!, CultureInfo.InvariantCulture), 3590, 3601);
    }

    private async Task<HttpResponseMessage> PutAsync(Uri url)
    {
        using var content = new ByteArrayContent(Jpeg);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        return await _browser.PutAsync(url, content, Ct);
    }
}
