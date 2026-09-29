using Buckl.Application.Common;
using Buckl.Application.Garments;
using Buckl.Application.Photos;
using Buckl.Application.Tests.Fakes;
using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Domain.Users;
using Buckl.Testing;
using Microsoft.Extensions.Logging.Abstractions;

namespace Buckl.Application.Tests.Garments;

public class UpdateGarmentHandlerTests
{
    private static readonly UserId Alice = UserId.New();

    private static readonly DateTimeOffset Later = TestClock.Now.AddDays(1);

    private readonly SpyUnitOfWork _unitOfWork = new();

    private readonly InMemoryPhotoStorage _storage = new();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task HandleAsync_changes_only_the_fields_the_command_sets()
    {
        var garment = TestGarments.Active(Alice, Category.Top, Color.Blue, purchaseInfo: TestGarments.Purchase());
        var repository = new InMemoryGarmentRepository(garment);
        var command = new UpdateGarmentCommand(garment.Id, default, default, new FieldUpdate<string?>("Linen"));

        var updated = await Handler(repository).HandleAsync(command, Ct);

        Assert.Equal("Linen", updated.Notes);
        Assert.Equal(Category.Top, updated.Classification.Category);
        Assert.NotNull(updated.PurchaseInfo);
        Assert.Equal(Later, updated.UpdatedAt);
        Assert.Equal(1, repository.UpdateCount);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_replaces_the_classification_when_it_is_set()
    {
        var garment = TestGarments.Active(Alice);
        var command = new UpdateGarmentCommand(
            garment.Id,
            new FieldUpdate<ClassificationInput>(new ClassificationInput(Category.Bottom, Color.Black, "32")),
            default,
            default);

        var updated = await Handler(new InMemoryGarmentRepository(garment)).HandleAsync(command, Ct);

        Assert.Equal(Classification.Create(Category.Bottom, Color.Black, Size.Create("32")), updated.Classification);
    }

    [Fact]
    public async Task HandleAsync_clears_the_purchase_when_it_is_set_to_null()
    {
        var garment = TestGarments.Active(Alice, purchaseInfo: TestGarments.Purchase());
        var command = new UpdateGarmentCommand(garment.Id, default, new FieldUpdate<PurchaseInput?>(null), default);

        var updated = await Handler(new InMemoryGarmentRepository(garment)).HandleAsync(command, Ct);

        Assert.Null(updated.PurchaseInfo);
    }

    [Fact]
    public async Task HandleAsync_rejects_editing_an_archived_garment_and_saves_nothing()
    {
        var garment = TestGarments.Archived(Alice);
        var command = new UpdateGarmentCommand(garment.Id, default, default, new FieldUpdate<string?>("x"));

        var exception = await Assert.ThrowsAsync<ArchivedGarmentIsReadOnlyException>(
            () => Handler(new InMemoryGarmentRepository(garment)).HandleAsync(command, Ct));

        Assert.Equal(ArchivedGarmentIsReadOnlyException.ErrorCode, exception.Code);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_rejects_an_archived_garment_even_when_nothing_changes()
    {
        var garment = TestGarments.Archived(Alice);
        var command = new UpdateGarmentCommand(garment.Id, default, default, default);

        await Assert.ThrowsAsync<ArchivedGarmentIsReadOnlyException>(
            () => Handler(new InMemoryGarmentRepository(garment)).HandleAsync(command, Ct));

        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_reports_an_archived_garment_before_an_invalid_change()
    {
        var garment = TestGarments.Archived(Alice);
        var command = new UpdateGarmentCommand(
            garment.Id,
            new FieldUpdate<ClassificationInput>(new ClassificationInput(Category.Top, Color.Blue, new string('M', 30))),
            default,
            default);

        await Assert.ThrowsAsync<ArchivedGarmentIsReadOnlyException>(
            () => Handler(new InMemoryGarmentRepository(garment)).HandleAsync(command, Ct));
    }

    [Fact]
    public async Task HandleAsync_reports_another_users_garment_as_not_found()
    {
        var bobsGarment = TestGarments.Active(UserId.New());
        var command = new UpdateGarmentCommand(bobsGarment.Id, default, default, new FieldUpdate<string?>("x"));

        await Assert.ThrowsAsync<GarmentNotFoundException>(
            () => Handler(new InMemoryGarmentRepository(bobsGarment)).HandleAsync(command, Ct));
        Assert.Null(bobsGarment.Notes);
    }

    [Fact]
    public async Task HandleAsync_validates_the_purchase_against_the_current_date()
    {
        var garment = TestGarments.Active(Alice);
        var tomorrow = DateOnly.FromDateTime(Later.UtcDateTime).AddDays(1);
        var command = new UpdateGarmentCommand(
            garment.Id,
            default,
            new FieldUpdate<PurchaseInput?>(new PurchaseInput(10m, "EUR", tomorrow)),
            default);

        var exception = await Assert.ThrowsAsync<DomainValidationException>(
            () => Handler(new InMemoryGarmentRepository(garment)).HandleAsync(command, Ct));

        Assert.Equal(PurchaseInfo.Errors.DateInFuture, exception.Code);
    }

    [Fact]
    public async Task HandleAsync_replaces_the_photo_and_lets_go_of_the_old_one()
    {
        var old = TestGarments.PhotoFor(Alice);
        _storage.Put(old.Value, 1000);
        var garment = TestGarments.Active(Alice, photoKey: old);
        var uploadId = Guid.NewGuid();
        _storage.Put(PhotoUploads.StagingKey(Alice, uploadId), 2000);
        var command = new UpdateGarmentCommand(garment.Id, default, default, default, new FieldUpdate<Guid?>(uploadId));

        var updated = await Handler(new InMemoryGarmentRepository(garment)).HandleAsync(command, Ct);

        Assert.Equal($"users/{Alice.Value:D}/garments/{uploadId:N}.jpg", updated.PhotoKey?.Value);
        Assert.False(_storage.Contains(old.Value));
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_removes_the_photo_when_it_is_set_to_null()
    {
        var old = TestGarments.PhotoFor(Alice);
        _storage.Put(old.Value, 1000);
        var garment = TestGarments.Active(Alice, photoKey: old);
        var command = new UpdateGarmentCommand(garment.Id, default, default, default, new FieldUpdate<Guid?>(null));

        var updated = await Handler(new InMemoryGarmentRepository(garment)).HandleAsync(command, Ct);

        Assert.Null(updated.PhotoKey);
        Assert.False(_storage.Contains(old.Value));
    }

    [Fact]
    public async Task HandleAsync_keeps_the_photo_when_the_command_leaves_it_alone()
    {
        var photo = TestGarments.PhotoFor(Alice);
        var garment = TestGarments.Active(Alice, photoKey: photo);
        var command = new UpdateGarmentCommand(garment.Id, default, default, new FieldUpdate<string?>("Linen"));

        var updated = await Handler(new InMemoryGarmentRepository(garment)).HandleAsync(command, Ct);

        Assert.Equal(photo, updated.PhotoKey);
        Assert.Empty(_storage.Deleted);
    }

    [Fact]
    public async Task HandleAsync_leaves_the_upload_alone_for_an_archived_garment()
    {
        var garment = TestGarments.Archived(Alice);
        var uploadId = Guid.NewGuid();
        var staging = PhotoUploads.StagingKey(Alice, uploadId);
        _storage.Put(staging, 2000);
        var command = new UpdateGarmentCommand(garment.Id, default, default, default, new FieldUpdate<Guid?>(uploadId));

        await Assert.ThrowsAsync<ArchivedGarmentIsReadOnlyException>(
            () => Handler(new InMemoryGarmentRepository(garment)).HandleAsync(command, Ct));

        Assert.True(_storage.Contains(staging));
    }

    private UpdateGarmentHandler Handler(InMemoryGarmentRepository repository) => new(
        repository,
        _unitOfWork,
        new FakeCurrentUser(Alice),
        new FixedTimeProvider(Later),
        new PhotoAttacher(_storage, NullLogger<PhotoAttacher>.Instance));
}
