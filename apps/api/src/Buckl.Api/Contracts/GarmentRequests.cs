using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Buckl.Application.Common;
using Buckl.Application.Garments;
using Buckl.Domain.Garments;

namespace Buckl.Api.Contracts;

public sealed class ClassificationRequest
{
    [Required]
    public Category? Category { get; init; }

    [Required]
    public Color? Color { get; init; }

    [JsonPropertyName("size")]
    public string? SizeLabel { get; init; }

    public ClassificationInput ToInput() => new(Category!.Value, Color!.Value, SizeLabel);
}

public sealed class MoneyRequest
{
    [Required]
    public decimal? Amount { get; init; }

    [Required]
    public string? Currency { get; init; }
}

public sealed class PurchaseInfoRequest
{
    [Required]
    public MoneyRequest? Price { get; init; }

    [Required]
    public DateOnly? Date { get; init; }

    public PurchaseInput ToInput() => new(Price!.Amount!.Value, Price.Currency!, Date!.Value);
}

/// <summary>A photo the browser already uploaded with a ticket from <c>POST /photos/uploads</c>
/// (ADR-0032).</summary>
public sealed class PhotoRequest
{
    [Required]
    public Guid? UploadId { get; init; }
}

/// <summary>Body of <c>POST /garments</c>, mirror of the web app's <c>NewGarment</c>; the photo
/// travels as the id of its upload.</summary>
public sealed class CreateGarmentRequest
{
    [Required]
    public ClassificationRequest? Classification { get; init; }

    public PurchaseInfoRequest? PurchaseInfo { get; init; }

    public string? Notes { get; init; }

    public PhotoRequest? Photo { get; init; }

    public CreateGarmentCommand ToCommand() =>
        new(Classification!.ToInput(), PurchaseInfo?.ToInput(), Notes, Photo?.UploadId);
}

/// <summary>Body of <c>PATCH /garments/{id}</c>, mirror of the web app's <c>GarmentChanges</c>:
/// an absent property is left alone, a null one is cleared. Classification cannot be
/// cleared.</summary>
public sealed class UpdateGarmentRequest : IValidatableObject
{
    public FieldUpdate<ClassificationRequest?> Classification { get; init; }

    public FieldUpdate<PurchaseInfoRequest?> PurchaseInfo { get; init; }

    public FieldUpdate<string?> Notes { get; init; }

    public FieldUpdate<PhotoRequest?> Photo { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Classification is { IsSet: true, Value: null })
        {
            yield return new ValidationResult(
                "A garment always has a classification; it cannot be cleared.",
                [nameof(Classification)]);
        }

        // [Required] on a nested type is not evaluated inside a FieldUpdate<T>, so it is checked here.
        if (Photo is { IsSet: true, Value: { UploadId: null } })
        {
            yield return new ValidationResult(
                "A photo is set by the id of its upload.",
                [nameof(Photo)]);
        }
    }

    public UpdateGarmentCommand ToCommand(GarmentId id) => new(
        id,
        Classification.IsSet
            ? new FieldUpdate<ClassificationInput>(Classification.Value!.ToInput())
            : default,
        PurchaseInfo.IsSet
            ? new FieldUpdate<PurchaseInput?>(PurchaseInfo.Value?.ToInput())
            : default,
        Notes,
        Photo.IsSet ? new FieldUpdate<Guid?>(Photo.Value?.UploadId) : default);
}
