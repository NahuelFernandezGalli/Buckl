using Buckl.Application.Common;
using Buckl.Domain.Garments;

namespace Buckl.Application.Garments;

/// <summary>A partial edit: only the fields that are set change. Classification cannot be
/// cleared, so when it is set its value is never null (the API validates that); purchase and
/// notes can be cleared with a null value.</summary>
public sealed record UpdateGarmentCommand(
    GarmentId Id,
    FieldUpdate<ClassificationInput> Classification,
    FieldUpdate<PurchaseInput?> Purchase,
    FieldUpdate<string?> Notes);
