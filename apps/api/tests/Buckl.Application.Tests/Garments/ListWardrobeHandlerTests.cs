using Buckl.Application.Garments;
using Buckl.Application.Tests.Fakes;
using Buckl.Domain.Garments;
using Buckl.Domain.Users;
using Buckl.Testing;

namespace Buckl.Application.Tests.Garments;

public class ListWardrobeHandlerTests
{
    private static readonly UserId Alice = UserId.New();

    private static readonly UserId Bob = UserId.New();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task HandleAsync_lists_only_the_current_users_garments()
    {
        var alicesGarment = TestGarments.Active(Alice);
        var repository = new InMemoryGarmentRepository(alicesGarment, TestGarments.Active(Bob));
        var handler = new ListWardrobeHandler(repository, new FakeCurrentUser(Alice));

        var wardrobe = await handler.HandleAsync(WardrobeFilter.Default, Ct);

        Assert.Equal(alicesGarment.Id, Assert.Single(wardrobe).Id);
    }

    [Fact]
    public async Task HandleAsync_passes_the_filter_to_the_repository_unchanged()
    {
        var repository = new InMemoryGarmentRepository();
        var handler = new ListWardrobeHandler(repository, new FakeCurrentUser(Alice));
        var filter = new WardrobeFilter { Category = Category.Bottom, SearchText = "denim" };

        await handler.HandleAsync(filter, Ct);

        Assert.Same(filter, repository.LastFilter);
    }
}
