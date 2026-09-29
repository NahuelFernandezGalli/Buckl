namespace Buckl.Application.Photos;

/// <summary>What the browser is about to upload, as it declares it. Checked again against the
/// stored object when a garment uses the photo.</summary>
public sealed record RequestPhotoUploadCommand(string ContentType, long Size);
