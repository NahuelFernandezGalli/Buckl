namespace Buckl.Application.Abstractions;

/// <summary>Object storage for garment photos (ADR-0032). The browser uploads and downloads
/// through the URLs this port signs; the API itself only inspects, copies and deletes. Keys are
/// opaque here: which key belongs to whom is the application's rule, never the adapter's.</summary>
public interface IPhotoStorage
{
    /// <summary>A URL the browser can PUT one object to, sending exactly
    /// <paramref name="contentType"/> as its <c>Content-Type</c>, until
    /// <paramref name="expiresAt"/>. Signing is local; storage is not called.</summary>
    Uri CreateUploadUrl(string key, string contentType, DateTimeOffset expiresAt);

    /// <summary>A URL the browser can GET the object from until <paramref name="expiresAt"/>.
    /// Signing is local; storage is not called.</summary>
    Uri CreateReadUrl(string key, DateTimeOffset expiresAt);

    /// <summary>Size and content type of a stored object, or null when there is none.</summary>
    Task<StoredObject?> FindAsync(string key, CancellationToken cancellationToken = default);

    Task CopyAsync(string sourceKey, string destinationKey, CancellationToken cancellationToken = default);

    /// <summary>Deleting a key that does not exist succeeds.</summary>
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>What storage knows about an object: its size in bytes and the content type it was
/// uploaded with.</summary>
public sealed record StoredObject(long Size, string? ContentType);

/// <summary>Storage could not be reached or refused an operation. Not mapped by the API, so a
/// request that needs storage and cannot reach it answers 500.</summary>
public sealed class PhotoStorageException : Exception
{
    public PhotoStorageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
