using Buckl.Domain.Common;
using Buckl.Domain.Users;

namespace Buckl.Domain.Garments;

/// <summary>Object key of a garment photo in storage. A key always lives under its owner's prefix
/// (<c>users/&lt;userId&gt;/</c>), so a garment can never point into another user's photos. The
/// API enforces the same prefix when it issues upload URLs; the domain enforces it again
/// here.</summary>
public sealed record PhotoKey
{
    public const int MaxLength = 512;

    private PhotoKey(string value, UserId ownerId)
    {
        Value = value;
        OwnerId = ownerId;
    }

    public string Value { get; }

    public UserId OwnerId { get; }

    private const string UsersRoot = "users/";

    public static string PrefixFor(UserId ownerId) => $"{UsersRoot}{ownerId.Value:D}/";

    public static PhotoKey Create(string value, UserId ownerId)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException(Errors.Empty, "Photo key cannot be empty.");
        }

        if (value.Length > MaxLength)
        {
            throw new DomainValidationException(
                Errors.TooLong,
                $"Photo key cannot exceed {MaxLength} characters.");
        }

        var prefix = PrefixFor(ownerId);

        if (!value.StartsWith(prefix, StringComparison.Ordinal) || value.Length == prefix.Length)
        {
            throw new DomainValidationException(
                Errors.OutsideOwnerPrefix,
                $"Photo key must be under '{prefix}'.");
        }

        return new PhotoKey(value, ownerId);
    }

    /// <summary>Rebuilds a stored key, reading its owner from the <c>users/&lt;userId&gt;/</c>
    /// prefix. Used when loading garments; the database stores only the key.</summary>
    public static PhotoKey Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException(Errors.Empty, "Photo key cannot be empty.");
        }

        return Create(value, OwnerFromPrefix(value));
    }

    private static UserId OwnerFromPrefix(string value)
    {
        var ownerEnd = value.StartsWith(UsersRoot, StringComparison.Ordinal)
            ? value.IndexOf('/', UsersRoot.Length)
            : -1;

        if (ownerEnd < 0
            || !Guid.TryParseExact(value.AsSpan(UsersRoot.Length, ownerEnd - UsersRoot.Length), "D", out var owner)
            || owner == Guid.Empty)
        {
            throw new DomainValidationException(
                Errors.OutsideOwnerPrefix,
                $"Photo key must be under '{UsersRoot}<userId>/'.");
        }

        return new UserId(owner);
    }

    public static class Errors
    {
        public const string Empty = "photo_key.empty";
        public const string TooLong = "photo_key.too_long";
        public const string OutsideOwnerPrefix = "photo_key.outside_owner_prefix";
    }
}
