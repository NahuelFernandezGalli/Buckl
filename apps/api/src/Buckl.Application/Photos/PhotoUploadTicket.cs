namespace Buckl.Application.Photos;

/// <summary>Where the browser uploads a photo: a PUT to <see cref="Url"/> with exactly
/// <see cref="ContentType"/>, before <see cref="ExpiresAt"/>. The garment then refers to it by
/// <see cref="UploadId"/>.</summary>
public sealed record PhotoUploadTicket(Guid UploadId, Uri Url, string ContentType, DateTimeOffset ExpiresAt);
