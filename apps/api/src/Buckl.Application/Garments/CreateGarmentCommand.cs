namespace Buckl.Application.Garments;

/// <summary>A garment added by hand, optionally with a photo the browser already uploaded
/// (ADR-0032).</summary>
public sealed record CreateGarmentCommand(
    ClassificationInput Classification,
    PurchaseInput? Purchase,
    string? Notes,
    Guid? PhotoUploadId = null);
