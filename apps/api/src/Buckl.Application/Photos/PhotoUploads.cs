using Buckl.Domain.Users;

namespace Buckl.Application.Photos;

/// <summary>How a photo travels from the browser to a garment (ADR-0032): the browser PUTs it to
/// a staging key the API signed for the caller, storage deletes staging objects after a day, and
/// attaching a photo moves it under the owner's photos.</summary>
public static class PhotoUploads
{
    /// <summary>How long an upload URL works: enough for a slow phone connection, short enough
    /// that a leaked URL is soon useless.</summary>
    public static readonly TimeSpan UploadWindow = TimeSpan.FromMinutes(10);

    /// <summary>How long a photo URL in a response works.</summary>
    public static readonly TimeSpan ReadWindow = TimeSpan.FromHours(1);

    /// <summary>Everything under this prefix is deleted by a storage lifecycle rule after one
    /// day (docs/configuration.md).</summary>
    public const string StagingRoot = "uploads/";

    /// <summary>Derived from the caller, never taken from a request: an upload id of another
    /// user leads to a key that does not exist.</summary>
    public static string StagingKey(UserId ownerId, Guid uploadId) =>
        $"{StagingRoot}{ownerId.Value:D}/{uploadId:D}";

    public static class Errors
    {
        public const string UploadNotFound = "photo.upload_not_found";
    }
}
