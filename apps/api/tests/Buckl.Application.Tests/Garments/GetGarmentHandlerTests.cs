using Buckl.Application.Garments;
using Buckl.Application.Tests.Fakes;
using Buckl.Domain.Garments;
using Buckl.Domain.Users;
using Buckl.Testing;

namespace Buckl.Application.Tests.Garments;

public class GetGarmentHandlerTests
{
    private static readonly UserId Alice = UserId.New();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task HandleAsync_returns_a_garment_of_the_current_user()
    {
        var garment = TestGarments.Active(Alice);
        var handler = new GetGarmentHandler(new InMemoryGarmentRepository(garment), new FakeCurrentUser(Alice));

        var found = await handler.HandleAsync(garment.Id, Ct);

        Assert.Same(garment, found);
    }

    [Fact]
    public async Task HandleAsync_reports_an_unknown_garment_as_not_found()
    {
        var handler = new GetGarmentHandler(new InMemoryGarmentRepository(), new FakeCurrentUser(Alice));
        var id = GarmentId.New();

        var exception = await Assert.ThrowsAsync<GarmentNotFoundException>(() => handler.HandleAsync(id, Ct));

        Assert.Equal(GarmentNotFoundException.ErrorCode, exception.Code);
        Assert.Equal(id, exception.GarmentId);
    }

    [Fact]
    public async Task HandleAsync_reports_another_users_garment_as_not_found()
    {
        var bobsGarment = TestGarments.Active(UserId.New());
        var handler = new GetGarmentHandler(new InMemoryGarmentRepository(bobsGarment), new FakeCurrentUser(Alice));

        await Assert.ThrowsAsync<GarmentNotFoundException>(() => handler.HandleAsync(bobsGarment.Id, Ct));
    }
}
