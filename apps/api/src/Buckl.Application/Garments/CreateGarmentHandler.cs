using Buckl.Application.Abstractions;
using Buckl.Domain.Common;
using Buckl.Domain.Garments;

namespace Buckl.Application.Garments;

/// <summary>Adds a garment to the current user's wardrobe.</summary>
public sealed class CreateGarmentHandler
{
    private readonly IGarmentRepository _garments;

    private readonly IUnitOfWork _unitOfWork;

    private readonly ICurrentUser _currentUser;

    private readonly TimeProvider _time;

    public CreateGarmentHandler(
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
        CreateGarmentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var now = _time.GetUtcNow();
        var garment = Garment.Create(
            _currentUser.Id,
            command.Classification.ToDomain(),
            ImportSource.Manual,
            now,
            purchaseInfo: command.Purchase?.ToDomain(now),
            notes: command.Notes);

        await _garments.AddAsync(garment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return garment;
    }
}
