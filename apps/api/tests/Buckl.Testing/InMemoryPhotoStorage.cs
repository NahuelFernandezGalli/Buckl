using System.Collections.Concurrent;
using Buckl.Application.Abstractions;

namespace Buckl.Testing;

/// <summary>Photo storage in memory, for every test that is not about the storage adapter
/// itself. Signed URLs are readable fakes (<c>https://photos.test/&lt;key&gt;?method=PUT…</c>) so
/// tests can assert what was signed; <see cref="Put"/> plays the browser's upload.</summary>
public sealed class InMemoryPhotoStorage : IPhotoStorage
{
    public static readonly Uri Root = new("https://photos.test/");

    private readonly ConcurrentDictionary<string, StoredObject> _objects = new(StringComparer.Ordinal);

    private readonly ConcurrentQueue<string> _deleted = new();

    private readonly ConcurrentQueue<IssuedUpload> _issuedUploads = new();

    /// <summary>When true, <see cref="DeleteAsync"/> fails the way an unreachable storage
    /// would.</summary>
    public bool FailDeletes { get; set; }

    /// <summary>When set, <see cref="CopyAsync"/> stores what this returns for the copied object
    /// instead of the object itself: the upload URL does not sign the size, so a browser can
    /// replace the staged object between a check and the copy.</summary>
    public Func<StoredObject, StoredObject>? OnCopy { get; set; }

    public IReadOnlyCollection<string> Deleted => [.. _deleted];

    public IReadOnlyCollection<IssuedUpload> IssuedUploads => [.. _issuedUploads];

    public static Uri UploadUrlFor(string key, string contentType, DateTimeOffset expiresAt) =>
        new(Root, $"{key}?method=PUT&type={Uri.EscapeDataString(contentType)}&expires={expiresAt.ToUnixTimeSeconds()}");

    public static Uri ReadUrlFor(string key, DateTimeOffset expiresAt) =>
        new(Root, $"{key}?method=GET&expires={expiresAt.ToUnixTimeSeconds()}");

    /// <summary>What the browser does with an upload URL.</summary>
    public void Put(string key, long size, string? contentType = "image/jpeg") =>
        _objects[key] = new StoredObject(size, contentType);

    public bool Contains(string key) => _objects.ContainsKey(key);

    public Uri CreateUploadUrl(string key, string contentType, DateTimeOffset expiresAt)
    {
        _issuedUploads.Enqueue(new IssuedUpload(key, contentType, expiresAt));

        return UploadUrlFor(key, contentType, expiresAt);
    }

    public Uri CreateReadUrl(string key, DateTimeOffset expiresAt) => ReadUrlFor(key, expiresAt);

    public Task<StoredObject?> FindAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(_objects.TryGetValue(key, out var stored) ? stored : null);

    public Task CopyAsync(string sourceKey, string destinationKey, CancellationToken cancellationToken = default)
    {
        if (!_objects.TryGetValue(sourceKey, out var stored))
        {
            throw new PhotoStorageException("The object to copy does not exist.", new KeyNotFoundException());
        }

        _objects[destinationKey] = OnCopy?.Invoke(stored) ?? stored;

        return Task.CompletedTask;
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        if (FailDeletes)
        {
            throw new PhotoStorageException("Storage is unreachable.", new TimeoutException());
        }

        _objects.TryRemove(key, out _);
        _deleted.Enqueue(key);

        return Task.CompletedTask;
    }
}

public sealed record IssuedUpload(string Key, string ContentType, DateTimeOffset ExpiresAt);
