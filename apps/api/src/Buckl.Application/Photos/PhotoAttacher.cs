using Buckl.Application.Abstractions;
using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Buckl.Application.Photos;

/// <summary>Turns an upload into a garment photo and lets go of photos no garment uses
/// (ADR-0032). Handlers call it after every other check of the request has passed, so a request
/// that is going to fail never moves a photo.</summary>
public sealed partial class PhotoAttacher
{
    private readonly IPhotoStorage _storage;

    private readonly ILogger<PhotoAttacher> _logger;

    public PhotoAttacher(IPhotoStorage storage, ILogger<PhotoAttacher> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    /// <summary>Checks what the owner uploaded with <paramref name="uploadId"/> against
    /// <see cref="PhotoFile"/>, using the stored size and type rather than what the browser
    /// declared, and moves it under the owner's photos. An upload that breaks the rules is
    /// deleted.</summary>
    public async Task<PhotoKey> AttachAsync(UserId ownerId, Guid uploadId, CancellationToken cancellationToken = default)
    {
        var stagingKey = PhotoUploads.StagingKey(ownerId, uploadId);
        var stored = await _storage.FindAsync(stagingKey, cancellationToken)
            ?? throw new DomainValidationException(
                PhotoUploads.Errors.UploadNotFound,
                "The photo upload was not found. Upload the photo again.");

        PhotoFile file;

        try
        {
            file = PhotoFile.Create(stored.ContentType ?? string.Empty, stored.Size);
        }
        catch (DomainValidationException)
        {
            await _storage.DeleteAsync(stagingKey, cancellationToken);
            throw;
        }

        var key = PhotoKey.ForGarmentPhoto(ownerId, uploadId, file);
        await _storage.CopyAsync(stagingKey, key.Value, cancellationToken);
        await _storage.DeleteAsync(stagingKey, cancellationToken);

        return key;
    }

    /// <summary>Deletes a photo no garment points at any more. A failure is logged and ignored:
    /// the edit the user asked for does not depend on it, and the object goes away with the
    /// account at the latest (docs/privacy.md).</summary>
    public async Task ReleaseAsync(PhotoKey key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        try
        {
            await _storage.DeleteAsync(key.Value, cancellationToken);
        }
        catch (PhotoStorageException exception)
        {
            LogReleaseFailed(_logger, exception);
        }
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "A garment photo that is no longer used could not be deleted; it stays in storage.")]
    private static partial void LogReleaseFailed(ILogger logger, Exception exception);
}
