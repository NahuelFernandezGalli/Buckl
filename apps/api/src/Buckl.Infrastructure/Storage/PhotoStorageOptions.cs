using System.Text.RegularExpressions;

namespace Buckl.Infrastructure.Storage;

/// <summary>Where garment photos live: an S3-compatible bucket, Backblaze B2 in every real
/// environment (ADR-0031, ADR-0032). <see cref="SecretAccessKey"/> is a secret; the rest is per environment.
/// Required everywhere: the API refuses to start without a complete, well-formed
/// section.</summary>
public sealed partial class PhotoStorageOptions
{
    public const string SectionName = "PhotoStorage";

    /// <summary>The bucket's S3 endpoint, such as
    /// <c>https://s3.us-east-005.backblazeb2.com</c>, without the bucket.</summary>
    public string ServiceUrl { get; set; } = string.Empty;

    /// <summary>The region in the endpoint, such as <c>us-east-005</c>. Requests signed for
    /// another region are refused.</summary>
    public string Region { get; set; } = string.Empty;

    public string Bucket { get; set; } = string.Empty;

    public string AccessKeyId { get; set; } = string.Empty;

    public string SecretAccessKey { get; set; } = string.Empty;

    /// <summary>Every problem with the settings, each naming its configuration key.</summary>
    public IEnumerable<string> Problems()
    {
        if (!Uri.TryCreate(ServiceUrl, UriKind.Absolute, out var url)
            || (url.Scheme != Uri.UriSchemeHttps && url.Scheme != Uri.UriSchemeHttp)
            || url.AbsolutePath != "/"
            || !string.IsNullOrEmpty(url.Query))
        {
            yield return "PhotoStorage:ServiceUrl must be the bucket's S3 endpoint, such as "
                + "https://s3.<region>.backblazeb2.com, without a path";
        }

        if (string.IsNullOrWhiteSpace(Region))
        {
            yield return "PhotoStorage:Region is missing";
        }

        if (!BucketName().IsMatch(Bucket))
        {
            yield return "PhotoStorage:Bucket must be a bucket name: 3 to 63 lower-case letters, digits and hyphens";
        }

        if (string.IsNullOrWhiteSpace(AccessKeyId))
        {
            yield return "PhotoStorage:AccessKeyId is missing";
        }

        if (string.IsNullOrWhiteSpace(SecretAccessKey))
        {
            yield return "PhotoStorage:SecretAccessKey is missing";
        }
    }

    [GeneratedRegex("^[a-z0-9][a-z0-9-]{1,61}[a-z0-9]$")]
    private static partial Regex BucketName();
}
