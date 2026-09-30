using Buckl.Application.Abstractions;
using Buckl.Application.Photos;
using Buckl.Domain.Garments;

namespace Buckl.Api.Photos;

/// <summary>Turns a stored photo key into the URL a browser loads it from: a presigned GET valid
/// for <see cref="PhotoUploads.ReadWindow"/>. Signing is local, so a whole wardrobe costs no
/// request to storage. Every response signs anew, so the URLs differ from one response to the
/// next (ADR-0032).</summary>
public sealed class PhotoUrls
{
    private readonly IPhotoStorage _storage;

    private readonly TimeProvider _time;

    public PhotoUrls(IPhotoStorage storage, TimeProvider time)
    {
        _storage = storage;
        _time = time;
    }

    public string? For(PhotoKey? key) =>
        key is null
            ? null
            : _storage.CreateReadUrl(key.Value, _time.GetUtcNow() + PhotoUploads.ReadWindow).AbsoluteUri;
}
