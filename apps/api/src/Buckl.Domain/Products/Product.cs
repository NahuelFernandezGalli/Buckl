using Buckl.Domain.Common;

namespace Buckl.Domain.Products;

/// <summary>A catalog article: the impersonal description of something a store sells. Products are
/// global rather than user-scoped, and immutable after creation in v1.</summary>
public sealed class Product
{
    public const int MaxNameLength = 200;

    public const int MaxBrandLength = 100;

    private Product(
        ProductId id,
        string name,
        string? brand,
        Uri? referenceImageUrl,
        Uri? sourceUrl,
        ImportSource source,
        DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        Brand = brand;
        ReferenceImageUrl = referenceImageUrl;
        SourceUrl = sourceUrl;
        Source = source;
        CreatedAt = createdAt;
    }

    public ProductId Id { get; }

    public string Name { get; }

    public string? Brand { get; }

    /// <summary>Public image from the store. Https only, because the web app embeds it.</summary>
    public Uri? ReferenceImageUrl { get; }

    /// <summary>Product page when imported from a URL. Http or https.</summary>
    public Uri? SourceUrl { get; }

    public ImportSource Source { get; }

    public DateTimeOffset CreatedAt { get; }

    /// <param name="name">Display name, required.</param>
    /// <param name="source">How the product was created.</param>
    /// <param name="now">The caller's current instant; stored in UTC.</param>
    /// <param name="brand">Optional brand; blank is stored as null.</param>
    /// <param name="referenceImageUrl">Optional absolute https image URL.</param>
    /// <param name="sourceUrl">Optional absolute http or https product page.</param>
    public static Product Create(
        string name,
        ImportSource source,
        DateTimeOffset now,
        string? brand = null,
        Uri? referenceImageUrl = null,
        Uri? sourceUrl = null)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (!Enum.IsDefined(source))
        {
            throw new DomainValidationException(
                Errors.UnknownSource,
                $"Unknown import source '{source}'.");
        }

        return new Product(
            ProductId.New(),
            NormalizeName(name),
            NormalizeBrand(brand),
            ValidateReferenceImageUrl(referenceImageUrl),
            ValidateSourceUrl(sourceUrl),
            source,
            now.ToUniversalTime());
    }

    private static string NormalizeName(string name)
    {
        var trimmed = name.Trim();

        if (trimmed.Length == 0)
        {
            throw new DomainValidationException(Errors.NameEmpty, "Product name cannot be empty.");
        }

        if (trimmed.Length > MaxNameLength)
        {
            throw new DomainValidationException(
                Errors.NameTooLong,
                $"Product name cannot exceed {MaxNameLength} characters.");
        }

        return trimmed;
    }

    private static string? NormalizeBrand(string? brand)
    {
        var trimmed = brand?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        if (trimmed.Length > MaxBrandLength)
        {
            throw new DomainValidationException(
                Errors.BrandTooLong,
                $"Brand cannot exceed {MaxBrandLength} characters.");
        }

        return trimmed;
    }

    private static Uri? ValidateReferenceImageUrl(Uri? url)
    {
        if (url is null)
        {
            return null;
        }

        if (!url.IsAbsoluteUri || url.Scheme != Uri.UriSchemeHttps)
        {
            throw new DomainValidationException(
                Errors.ImageUrlNotHttps,
                "Reference image URL must be an absolute https URL.");
        }

        return url;
    }

    private static Uri? ValidateSourceUrl(Uri? url)
    {
        if (url is null)
        {
            return null;
        }

        if (!url.IsAbsoluteUri
            || (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainValidationException(
                Errors.SourceUrlInvalid,
                "Source URL must be an absolute http or https URL.");
        }

        return url;
    }

    public static class Errors
    {
        public const string NameEmpty = "product.name_empty";
        public const string NameTooLong = "product.name_too_long";
        public const string BrandTooLong = "product.brand_too_long";
        public const string ImageUrlNotHttps = "product.image_url_not_https";
        public const string SourceUrlInvalid = "product.source_url_invalid";
        public const string UnknownSource = "product.unknown_source";
    }
}
