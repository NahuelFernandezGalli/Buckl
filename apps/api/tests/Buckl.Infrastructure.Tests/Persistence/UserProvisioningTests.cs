using Buckl.Domain.Users;
using Buckl.Infrastructure.Persistence;
using Buckl.Testing;
using Microsoft.EntityFrameworkCore;

namespace Buckl.Infrastructure.Tests.Persistence;

public class UserProvisioningTests
{
    private readonly PostgresDatabase _database;

    public UserProvisioningTests(PostgresDatabase database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task The_first_request_of_a_subject_creates_its_user()
    {
        var subject = $"dev|{Guid.NewGuid():N}";

        var id = await EnsureAsync(subject);

        await using var owner = _database.CreateOwnerContext();
        var stored = await owner.Users.SingleAsync(user => user.Auth0Subject == subject, Ct);
        Assert.Equal(id.Value, stored.Id);
        Assert.Equal(TestClock.Now, stored.CreatedAt);
    }

    [Fact]
    public async Task The_same_subject_always_maps_to_the_same_user()
    {
        var subject = $"dev|{Guid.NewGuid():N}";

        var first = await EnsureAsync(subject);
        var second = await EnsureAsync(subject);

        Assert.Equal(first, second);
    }

    [Fact]
    public async Task Different_subjects_map_to_different_users()
    {
        var alice = await EnsureAsync($"dev|{Guid.NewGuid():N}");
        var bob = await EnsureAsync($"dev|{Guid.NewGuid():N}");

        Assert.NotEqual(alice, bob);
    }

    [Fact]
    public async Task Concurrent_first_requests_of_a_subject_agree_on_one_user()
    {
        var subject = $"dev|{Guid.NewGuid():N}";

        var ids = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => EnsureAsync(subject)));

        Assert.Single(ids.Distinct());
    }

    private async Task<UserId> EnsureAsync(string subject)
    {
        // As the application role and outside any user transaction, the way the API calls it.
        await using var context = _database.CreateAppContext();

        return await new EfUserProvisioning(context, new FixedTimeProvider(TestClock.Now))
            .EnsureUserAsync(subject, Ct);
    }
}
