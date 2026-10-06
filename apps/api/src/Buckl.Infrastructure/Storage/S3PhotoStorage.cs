using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Buckl.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace Buckl.Infrastructure.Storage;

/// <summary>Garment photos in an S3-compatible bucket: Backblaze B2 (ADR-0031, ADR-0032). One client for
/// the life of the process; signing is local, the other operations are one request each. Errors
/// never carry the object key, which contains the user's id.</summary>
public sealed class S3PhotoStorage : IPhotoStorage, IDisposable
{
    private readonly AmazonS3Client _client;

    private readonly string _bucket;

    private readonly Protocol _protocol;

    public S3PhotoStorage(IOptions<PhotoStorageOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var settings = options.Value;
        _bucket = settings.Bucket;
        _protocol = new Uri(settings.ServiceUrl).Scheme == Uri.UriSchemeHttp ? Protocol.HTTP : Protocol.HTTPS;
        _client = new AmazonS3Client(
            new BasicAWSCredentials(settings.AccessKeyId, settings.SecretAccessKey),
            new AmazonS3Config
            {
                ServiceURL = settings.ServiceUrl,
                AuthenticationRegion = settings.Region,
                ForcePathStyle = true,
                // Newer SDKs add CRC32 checksums to every upload, presigned ones included, and not
                // every S3-compatible store accepts them. Only send one when an operation requires it.
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
                Timeout = TimeSpan.FromSeconds(10),
                MaxErrorRetry = 2,
            });
    }

    public Uri CreateUploadUrl(string key, string contentType, DateTimeOffset expiresAt) =>
        Presign(key, HttpVerb.PUT, expiresAt, contentType);

    public Uri CreateReadUrl(string key, DateTimeOffset expiresAt) =>
        Presign(key, HttpVerb.GET, expiresAt, contentType: null);

    public async Task<StoredObject?> FindAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var metadata = await _client.GetObjectMetadataAsync(_bucket, key, cancellationToken);

            return new StoredObject(metadata.ContentLength, metadata.Headers.ContentType);
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception exception) when (IsStorageFailure(exception))
        {
            throw new PhotoStorageException("Photo storage could not describe an object.", exception);
        }
    }

    public async Task CopyAsync(string sourceKey, string destinationKey, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.CopyObjectAsync(
                new CopyObjectRequest
                {
                    SourceBucket = _bucket,
                    SourceKey = sourceKey,
                    DestinationBucket = _bucket,
                    DestinationKey = destinationKey,
                },
                cancellationToken);
        }
        catch (Exception exception) when (IsStorageFailure(exception))
        {
            throw new PhotoStorageException("Photo storage could not copy an object.", exception);
        }
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.DeleteObjectAsync(_bucket, key, cancellationToken);
        }
        catch (Exception exception) when (IsStorageFailure(exception))
        {
            throw new PhotoStorageException("Photo storage could not delete an object.", exception);
        }
    }

    public void Dispose() => _client.Dispose();

    private Uri Presign(string key, HttpVerb verb, DateTimeOffset expiresAt, string? contentType) =>
        new(_client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = key,
            Verb = verb,
            ContentType = contentType,
            Expires = expiresAt.UtcDateTime,
            Protocol = _protocol,
        }));

    private static bool IsStorageFailure(Exception exception) =>
        exception is AmazonServiceException or AmazonClientException or HttpRequestException;
}
