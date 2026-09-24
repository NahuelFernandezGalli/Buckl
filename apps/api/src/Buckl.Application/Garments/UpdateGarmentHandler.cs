using Buckl.Application.Abstractions;
using Buckl.Domain.Garments;

namespace Buckl.Application.Garments;

/// <summary>Edits some fields of one of the current user's garments.</summary>
public sealed class UpdateGarmentHandler
{
    private readonly IGarmentRepository _garments;

    private readonly IUnitOfWork _unitOfWork;

    private readonly ICurrentUser _currentUser;

    private readonly TimeProvider _time;

    public UpdateGarmentHandler(
        IGarmentRepository garments,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        TimeProvider time)
    {
        _garments = garments;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _time = time;
    }

    public async Task<Garment> HandleAsync(
        UpdateGarmentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var garment = await _garments.GetOwnedAsync(command.Id, _currentUser.Id, cancellationToken);
        var now = _time.GetUtcNow();

        if (command.Classification.IsSet)
        {
            garment.UpdateClassification(command.Classification.Value.ToDomain(), now);
        }

        if (command.Purchase.IsSet)
        {
            garment.UpdatePurchaseInfo(command.Purchase.Value?.ToDomain(now), now);
        }

        if (command.Notes.IsSet)
        {
            garment.UpdateNotes(command.Notes.Value, now);
        }

        await _garments.UpdateAsync(garment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return garment;
    }
}
