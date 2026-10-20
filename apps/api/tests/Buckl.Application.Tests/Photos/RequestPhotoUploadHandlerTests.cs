using Buckl.Application.Photos;
using Buckl.Application.Tests.Fakes;
using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Domain.Users;
using Buckl.Testing;

namespace Buckl.Application.Tests.Photos;

public class RequestPhotoUploadHandlerTests
{
    private static readonly UserId Alice = UserId.New();

    private readonly InMemoryPhotoStorage _storage = new();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task HandleAsync_signs_an_upload_to_the_callers_staging_area_for_ten_minutes()
    {
        var ticket = await Handler().HandleAsync(new RequestPhotoUploadCommand("IMAGE/JPEG", 200_000), Ct);

        var issued = Assert.Single(_storage.IssuedUploads);
        Assert.Equal($"uploads/{Alice.Value:D}/{ticket.UploadId:D}", issued.Key);
        Assert.Equal("image/jpeg", issued.ContentType);
        Assert.Equal("image/jpeg", ticket.ContentType);
        Assert.Equal(TestClock.Now.AddMinutes(10), ticket.ExpiresAt);
        Assert.Equal(InMemoryPhotoStorage.UploadUrlFor(issued.Key, "image/jpeg", ticket.ExpiresAt), ticket.Url);
    }

    [Fact]
    public async Task HandleAsync_gives_every_upload_its_own_id()
    {
        var first = await Handler().HandleAsync(new RequestPhotoUploadCommand("image/jpeg", 10), Ct);
        var second = await Handler().HandleAsync(new RequestPhotoUploadCommand("image/jpeg", 10), Ct);

        Assert.NotEqual(Guid.Empty, first.UploadId);
        Assert.NotEqual(first.UploadId, second.UploadId);
    }

    [Theory]
    [InlineData("image/gif", 1000L, PhotoFile.Errors.UnsupportedType)]
    [InlineData("image/jpeg", 0L, PhotoFile.Errors.Empty)]
    [InlineData("image/png", PhotoFile.MaxBytes + 1, PhotoFile.Errors.TooLarge)]
    public async Task HandleAsync_signs_nothing_for_a_photo_the_rules_reject(string contentType, long size, string code)
    {
        var exception = await Assert.ThrowsAsync<DomainValidationException>(
            () => Handler().HandleAsync(new RequestPhotoUploadCommand(contentType, size), Ct));

        Assert.Equal(code, exception.Code);
        Assert.Empty(_storage.IssuedUploads);
    }

    private RequestPhotoUploadHandler Handler() => new(
        _storage,
        new FakeCurrentUser(Alice),
        new FixedTimeProvider(TestClock.Now));
}
