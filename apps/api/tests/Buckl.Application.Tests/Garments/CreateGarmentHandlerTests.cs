using Buckl.Application.Garments;
using Buckl.Application.Tests.Fakes;
using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Domain.Users;
using Buckl.Testing;

namespace Buckl.Application.Tests.Garments;

public class CreateGarmentHandlerTests
{
    private static readonly UserId Alice = UserId.New();

    private readonly InMemoryGarmentRepository _repository = new();

    private readonly SpyUnitOfWork _unitOfWork = new();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task HandleAsync_stores_a_manual_garment_of_the_current_user()
    {
        var command = new CreateGarmentCommand(
            new ClassificationInput(Category.Top, Color.Blue, "M"),
            Purchase: null,
            Notes: "  Oxford shirt  ");

        var garment = await Handler().HandleAsync(command, Ct);

        Assert.Same(garment, Assert.Single(_repository.Stored));
        Assert.Equal(Alice, garment.OwnerId);
        Assert.Equal(ImportSource.Manual, garment.Source);
        Assert.Equal(Classification.Create(Category.Top, Color.Blue, Size.Create("M")), garment.Classification);
        Assert.Equal("Oxford shirt", garment.Notes);
        Assert.Equal(TestClock.Now, garment.CreatedAt);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_treats_a_blank_size_as_no_size()
    {
        var command = new CreateGarmentCommand(new ClassificationInput(Category.Top, Color.Blue, "  "), null, null);

        var garment = await Handler().HandleAsync(command, Ct);

        Assert.Null(garment.Classification.Size);
    }

    [Fact]
    public async Task HandleAsync_accepts_a_purchase_made_today()
    {
        var command = new CreateGarmentCommand(
            new ClassificationInput(Category.Top, Color.Blue, null),
            new PurchaseInput(49.90m, "usd", TestClock.Today),
            Notes: null);

        var garment = await Handler().HandleAsync(command, Ct);

        Assert.Equal(Money.Create(49.90m, "USD"), garment.PurchaseInfo!.Price);
        Assert.Equal(TestClock.Today, garment.PurchaseInfo.Date);
    }

    [Fact]
    public async Task HandleAsync_rejects_a_purchase_dated_after_today_and_stores_nothing()
    {
        var command = new CreateGarmentCommand(
            new ClassificationInput(Category.Top, Color.Blue, null),
            new PurchaseInput(49.90m, "USD", TestClock.Today.AddDays(1)),
            Notes: null);

        var exception = await Assert.ThrowsAsync<DomainValidationException>(() => Handler().HandleAsync(command, Ct));

        Assert.Equal(PurchaseInfo.Errors.DateInFuture, exception.Code);
        Assert.Empty(_repository.Stored);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    private CreateGarmentHandler Handler() => new(
        _repository,
        _unitOfWork,
        new FakeCurrentUser(Alice),
        new FixedTimeProvider(TestClock.Now));
}
