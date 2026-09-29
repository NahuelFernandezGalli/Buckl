using Buckl.Application.Abstractions;
using Buckl.Application.Photos;
using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Domain.Users;
using Buckl.Testing;
using Microsoft.Extensions.Logging.Abstractions;

namespace Buckl.Application.Tests.Photos;

public class PhotoAttacherTests
{
    private static readonly UserId Alice = UserId.New();

    private readonly InMemoryPhotoStorage _storage = new();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private PhotoAttacher Attacher => new(_storage, NullLogger<PhotoAttacher>.Instance);

    [Fact]
    public async Task AttachAsync_moves_the_upload_under_the_owners_photos()
    {
        var uploadId = Guid.NewGuid();
        var staging = PhotoUploads.StagingKey(Alice, uploadId);
        _storage.Put(staging, 180_000, "image/jpeg");

        var key = await Attacher.AttachAsync(Alice, uploadId, Ct);

        Assert.Equal($"users/{Alice.Value:D}/garments/{uploadId:N}.jpg", key.Value);
        Assert.True(_storage.Contains(key.Value));
        Assert.False(_storage.Contains(staging));
    }

    [Fact]
    public async Task AttachAsync_names_the_photo_after_what_was_actually_stored()
    {
        var uploadId = Guid.NewGuid();
        _storage.Put(PhotoUploads.StagingKey(Alice, uploadId), 90_000, "IMAGE/PNG");

        var key = await Attacher.AttachAsync(Alice, uploadId, Ct);

        Assert.EndsWith(".png", key.Value, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AttachAsync_does_not_find_an_upload_that_never_happened()
    {
        var exception = await Assert.ThrowsAsync<DomainValidationException>(
            () => Attacher.AttachAsync(Alice, Guid.NewGuid(), Ct));

        Assert.Equal(PhotoUploads.Errors.UploadNotFound, exception.Code);
    }

    [Fact]
    public async Task AttachAsync_does_not_find_another_users_upload()
    {
        var uploadId = Guid.NewGuid();
        var bobs = PhotoUploads.StagingKey(UserId.New(), uploadId);
        _storage.Put(bobs, 1000);

        var exception = await Assert.ThrowsAsync<DomainValidationException>(
            () => Attacher.AttachAsync(Alice, uploadId, Ct));

        Assert.Equal(PhotoUploads.Errors.UploadNotFound, exception.Code);
        Assert.True(_storage.Contains(bobs));
    }

    [Theory]
    [InlineData(PhotoFile.MaxBytes + 1, "image/jpeg", PhotoFile.Errors.TooLarge)]
    [InlineData(1000L, "image/gif", PhotoFile.Errors.UnsupportedType)]
    [InlineData(1000L, null, PhotoFile.Errors.UnsupportedType)]
    public async Task AttachAsync_discards_an_upload_that_breaks_the_rules(long size, string? contentType, string code)
    {
        var uploadId = Guid.NewGuid();
        var staging = PhotoUploads.StagingKey(Alice, uploadId);
        _storage.Put(staging, size, contentType);

        var exception = await Assert.ThrowsAsync<DomainValidationException>(
            () => Attacher.AttachAsync(Alice, uploadId, Ct));

        Assert.Equal(code, exception.Code);
        Assert.False(_storage.Contains(staging));
    }

    [Fact]
    public async Task AttachAsync_consumes_an_upload_only_once()
    {
        var uploadId = Guid.NewGuid();
        var staging = PhotoUploads.StagingKey(Alice, uploadId);
        _storage.Put(staging, 1000);
        var key = await Attacher.AttachAsync(Alice, uploadId, Ct);
        _storage.Put(staging, 2000);

        var exception = await Assert.ThrowsAsync<DomainValidationException>(
            () => Attacher.AttachAsync(Alice, uploadId, Ct));

        Assert.Equal(PhotoUploads.Errors.UploadNotFound, exception.Code);
        Assert.True(_storage.Contains(key.Value));
        Assert.True(_storage.Contains(staging));
        Assert.DoesNotContain(key.Value, _storage.Deleted);
    }

    [Fact]
    public async Task AttachAsync_checks_again_what_the_copy_actually_holds()
    {
        var uploadId = Guid.NewGuid();
        var staging = PhotoUploads.StagingKey(Alice, uploadId);
        _storage.Put(staging, 1000);
        _storage.OnCopy = _ => new StoredObject(PhotoFile.MaxBytes + 1, "image/jpeg");

        var exception = await Assert.ThrowsAsync<DomainValidationException>(
            () => Attacher.AttachAsync(Alice, uploadId, Ct));

        Assert.Equal(PhotoFile.Errors.TooLarge, exception.Code);
        Assert.False(_storage.Contains(staging));
        Assert.False(_storage.Contains($"users/{Alice.Value:D}/garments/{uploadId:N}.jpg"));
    }

    [Fact]
    public async Task ReleaseAsync_deletes_a_photo_nobody_uses()
    {
        var key = TestGarments.PhotoFor(Alice);
        _storage.Put(key.Value, 1000);

        await Attacher.ReleaseAsync(key, Ct);

        Assert.False(_storage.Contains(key.Value));
    }

    [Fact]
    public async Task ReleaseAsync_does_not_fail_the_request_when_storage_is_down()
    {
        _storage.FailDeletes = true;

        await Attacher.ReleaseAsync(TestGarments.PhotoFor(Alice), Ct);
    }
}
