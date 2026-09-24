using Buckl.Application.Abstractions;
using Buckl.Domain.Garments;

namespace Buckl.Application.Garments;

/// <summary>Hides one of the current user's garments from the wardrobe.</summary>
public sealed class ArchiveGarmentHandler
{
    private readonly IGarmentRepository _garments;

    private readonly IUnitOfWork _unitOfWork;

    private readonly ICurrentUser _currentUser;

    private readonly TimeProvider _time;

    public ArchiveGarmentHandler(
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

    public async Task<Garment> HandleAsync(GarmentId id, CancellationToken cancellationToken = default)
    {
        var garment = await _garments.GetOwnedAsync(id, _currentUser.Id, cancellationToken);
        garment.Archive(_time.GetUtcNow());

        await _garments.UpdateAsync(garment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return garment;
    }
}
