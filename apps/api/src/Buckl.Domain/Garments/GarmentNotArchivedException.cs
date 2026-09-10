using Buckl.Domain.Common;

namespace Buckl.Domain.Garments;

/// <summary>Raised when a garment is in the wrong state for the requested transition.</summary>
public sealed class GarmentNotArchivedException : DomainException
{
    public const string ErrorCode = "garment.not_archived";

    public GarmentNotArchivedException(GarmentId garmentId)
        : base(ErrorCode, $"Garment {garmentId} is not archived.")
    {
        GarmentId = garmentId;
    }

    public GarmentId GarmentId { get; }
}
