using Buckl.Application.Abstractions;
using Buckl.Domain.Garments;

namespace Buckl.Application.Photos;

/// <summary>Signs an upload for the current user. Nothing is stored: the object exists only once
/// the browser uploads it, and disappears after a day unless a garment starts using it.</summary>
public sealed class RequestPhotoUploadHandler
{
    private readonly IPhotoStorage _photos;

    private readonly ICurrentUser _currentUser;

    private readonly TimeProvider _time;

    public RequestPhotoUploadHandler(IPhotoStorage photos, ICurrentUser currentUser, TimeProvider time)
    {
        _photos = photos;
        _currentUser = currentUser;
        _time = time;
    }

    public Task<PhotoUploadTicket> HandleAsync(
        RequestPhotoUploadCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var file = PhotoFile.Create(command.ContentType, command.Size);
        var uploadId = Guid.NewGuid();
        var expiresAt = _time.GetUtcNow() + PhotoUploads.UploadWindow;
        var url = _photos.CreateUploadUrl(
            PhotoUploads.StagingKey(_currentUser.Id, uploadId),
            file.ContentType,
            expiresAt);

        return Task.FromResult(new PhotoUploadTicket(uploadId, url, file.ContentType, expiresAt));
    }
}
