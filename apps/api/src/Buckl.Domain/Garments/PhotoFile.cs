using Buckl.Domain.Common;

namespace Buckl.Domain.Garments;

/// <summary>What a garment photo may be: a JPEG, PNG or WebP image of at most 5 MiB. Checked when
/// the browser asks to upload one, against what it declares, and again against the stored object
/// before a garment points at it. The web app sends 1080×1350 JPEGs of a few hundred kilobytes;
/// the limit leaves room without letting storage fill up.</summary>
public sealed record PhotoFile
{
    public const long MaxBytes = 5 * 1024 * 1024;

    private static readonly (string ContentType, string Extension)[] Accepted =
    [
        ("image/jpeg", "jpg"),
        ("image/png", "png"),
        ("image/webp", "webp"),
    ];

    private PhotoFile(string contentType, string extension, long size)
    {
        ContentType = contentType;
        Extension = extension;
        Size = size;
    }

    /// <summary>The canonical, lower-case media type.</summary>
    public string ContentType { get; }

    public string Extension { get; }

    public long Size { get; }

    public static PhotoFile Create(string contentType, long size)
    {
        ArgumentNullException.ThrowIfNull(contentType);

        var trimmed = contentType.Trim();
        var accepted = Array.Find(
            Accepted,
            candidate => string.Equals(candidate.ContentType, trimmed, StringComparison.OrdinalIgnoreCase));

        if (accepted.ContentType is null)
        {
            throw new DomainValidationException(
                Errors.UnsupportedType,
                "A photo must be a JPEG, PNG or WebP image.");
        }

        if (size <= 0)
        {
            throw new DomainValidationException(Errors.Empty, "A photo cannot be empty.");
        }

        if (size > MaxBytes)
        {
            throw new DomainValidationException(
                Errors.TooLarge,
                $"A photo cannot exceed {MaxBytes} bytes.");
        }

        return new PhotoFile(accepted.ContentType, accepted.Extension, size);
    }

    public static class Errors
    {
        public const string UnsupportedType = "photo.unsupported_type";
        public const string Empty = "photo.empty";
        public const string TooLarge = "photo.too_large";
    }
}
