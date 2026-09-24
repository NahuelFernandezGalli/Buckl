namespace Buckl.Application.Garments;

/// <summary>A garment added by hand. Photos are attached in phase 6, not at creation.</summary>
public sealed record CreateGarmentCommand(
    ClassificationInput Classification,
    PurchaseInput? Purchase,
    string? Notes);
