using Buckl.Application.Garments;
using Buckl.Application.Tests.Fakes;
using Buckl.Domain.Garments;
using Buckl.Domain.Users;
using Buckl.Testing;

namespace Buckl.Application.Tests.Garments;

public class ArchiveAndRestoreGarmentHandlerTests
{
    private static readonly UserId Alice = UserId.New();

    private static readonly DateTimeOffset Later = TestClock.Now.AddDays(1);

    private readonly SpyUnitOfWork _unitOfWork = new();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Archive_archives_the_garment_and_saves()
    {
        var garment = TestGarments.Active(Alice);

        var archived = await ArchiveHandler(garment).HandleAsync(garment.Id, Ct);

        Assert.True(archived.IsArchived);
        Assert.Equal(Later, archived.ArchivedAt);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Archive_rejects_an_archived_garment()
    {
        var garment = TestGarments.Archived(Alice);

        var exception = await Assert.ThrowsAsync<GarmentAlreadyArchivedException>(
            () => ArchiveHandler(garment).HandleAsync(garment.Id, Ct));

        Assert.Equal(GarmentAlreadyArchivedException.ErrorCode, exception.Code);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Restore_brings_an_archived_garment_back_and_saves()
    {
        var garment = TestGarments.Archived(Alice);

        var restored = await RestoreHandler(garment).HandleAsync(garment.Id, Ct);

        Assert.False(restored.IsArchived);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Restore_rejects_an_active_garment()
    {
        var garment = TestGarments.Active(Alice);

        var exception = await Assert.ThrowsAsync<GarmentNotArchivedException>(
            () => RestoreHandler(garment).HandleAsync(garment.Id, Ct));

        Assert.Equal(GarmentNotArchivedException.ErrorCode, exception.Code);
    }

    [Fact]
    public async Task Archive_reports_another_users_garment_as_not_found()
    {
        var bobsGarment = TestGarments.Active(UserId.New());

        await Assert.ThrowsAsync<GarmentNotFoundException>(
            () => ArchiveHandler(bobsGarment).HandleAsync(bobsGarment.Id, Ct));
        Assert.False(bobsGarment.IsArchived);
    }

    private ArchiveGarmentHandler ArchiveHandler(params Garment[] garments) => new(
        new InMemoryGarmentRepository(garments),
        _unitOfWork,
        new FakeCurrentUser(Alice),
        new FixedTimeProvider(Later));

    private RestoreGarmentHandler RestoreHandler(params Garment[] garments) => new(
        new InMemoryGarmentRepository(garments),
        _unitOfWork,
        new FakeCurrentUser(Alice),
        new FixedTimeProvider(Later));
}
