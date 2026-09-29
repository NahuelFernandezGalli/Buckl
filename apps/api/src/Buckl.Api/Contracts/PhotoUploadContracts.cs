using System.ComponentModel.DataAnnotations;
using Buckl.Application.Photos;

namespace Buckl.Api.Contracts;

/// <summary>Body of <c>POST /photos/uploads</c>: what the browser is about to upload.</summary>
public sealed class PhotoUploadRequest
{
    [Required]
    public string? ContentType { get; init; }

    [Required]
    public long? Size { get; init; }

    public RequestPhotoUploadCommand ToCommand() => new(ContentType!, Size!.Value);
}

/// <summary>How the browser uploads the photo: <see cref="Method"/> to <see cref="Url"/> with
/// exactly <see cref="Headers"/> (the content type is part of the signature), before
/// <see cref="ExpiresAt"/>. No token: the URL itself is the credential.</summary>
public sealed record PhotoUploadResponse(
    Guid UploadId,
    Uri Url,
    string Method,
    IReadOnlyDictionary<string, string> Headers,
    DateTimeOffset ExpiresAt)
{
    public static PhotoUploadResponse From(PhotoUploadTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);

        return new PhotoUploadResponse(
            ticket.UploadId,
            ticket.Url,
            "PUT",
            new Dictionary<string, string> { ["Content-Type"] = ticket.ContentType },
            ticket.ExpiresAt);
    }
}
