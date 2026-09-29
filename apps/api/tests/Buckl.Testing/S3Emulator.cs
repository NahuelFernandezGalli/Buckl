using Amazon.Runtime;
using Amazon.S3;
using Buckl.Infrastructure.Storage;
using Testcontainers.Floci;
using Xunit;

namespace Buckl.Testing;

/// <summary>An S3-compatible emulator (Floci) with one empty bucket, for the storage adapter's
/// own tests. B2 is S3-compatible too, but not identical: what only B2 does (CORS, rejecting a PUT
/// with another content type) is checked by hand against the development bucket.</summary>
public sealed class S3Emulator : IAsyncLifetime
{
    public const string Bucket = "buckl-photos-test";

    private readonly FlociContainer _container = new FlociBuilder("floci/floci:1.5.13").Build();

    public string ServiceUrl =>
        $"http://{_container.Hostname}:{_container.GetMappedPublicPort(FlociBuilder.FlociPort)}";

    public PhotoStorageOptions Options => new()
    {
        ServiceUrl = ServiceUrl,
        Region = FlociBuilder.Region,
        Bucket = Bucket,
        AccessKeyId = FlociBuilder.AccessKey,
        SecretAccessKey = FlociBuilder.SecretKey,
    };

    /// <summary>A client of its own, to arrange and inspect objects behind the adapter's
    /// back.</summary>
    public AmazonS3Client CreateClient() => new(
        new BasicAWSCredentials(FlociBuilder.AccessKey, FlociBuilder.SecretKey),
        new AmazonS3Config { ServiceURL = ServiceUrl, AuthenticationRegion = FlociBuilder.Region, ForcePathStyle = true });

    public async ValueTask InitializeAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await _container.StartAsync(cancellationToken);

        using var client = CreateClient();
        await client.PutBucketAsync(Bucket, cancellationToken);
    }

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
