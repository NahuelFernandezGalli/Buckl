using Buckl.Domain.Garments;
using Buckl.Domain.Users;
using Buckl.Infrastructure.Persistence;
using Buckl.Infrastructure.Persistence.Records;
using Buckl.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Buckl.Infrastructure.Tests.Persistence;

public class UserTransactionTests
{
    private readonly PostgresDatabase _database;

    public UserTransactionTests(PostgresDatabase database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task A_user_transaction_scopes_every_query_to_that_user()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var bob = await _database.InsertUserAsync(Ct);
        var alicesGarment = await _database.InsertGarmentAsync(alice, Ct);
        var bobsGarment = await _database.InsertGarmentAsync(bob, Ct);
        await using var context = _database.CreateAppContext();
        await using var transaction = await new EfUserTransactionFactory(context).BeginAsync(alice, Ct);

        var visible = await context.Garments
            .Where(garment => garment.Id == alicesGarment.Value || garment.Id == bobsGarment.Value)
            .Select(garment => garment.Id)
            .ToListAsync(Ct);

        Assert.Equal(alicesGarment.Value, Assert.Single(visible));
    }

    [Fact]
    public async Task Changes_saved_through_the_unit_of_work_are_stored_on_commit()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var id = GarmentId.New();

        await using (var context = _database.CreateAppContext())
        {
            await using var transaction = await new EfUserTransactionFactory(context).BeginAsync(alice, Ct);
            context.Garments.Add(NewRecord(id, alice));
            await new EfUnitOfWork(context).SaveChangesAsync(Ct);
            await transaction.CommitAsync(Ct);
        }

        Assert.True(await _database.GarmentExistsAsync(id, Ct));
    }

    [Fact]
    public async Task Disposing_the_transaction_without_commit_rolls_back()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var id = GarmentId.New();

        await using (var context = _database.CreateAppContext())
        {
            await using var transaction = await new EfUserTransactionFactory(context).BeginAsync(alice, Ct);
            context.Garments.Add(NewRecord(id, alice));
            await new EfUnitOfWork(context).SaveChangesAsync(Ct);
        }

        Assert.False(await _database.GarmentExistsAsync(id, Ct));
    }

    [Fact]
    public async Task Saving_a_garment_for_another_user_is_rejected_by_the_policy()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var bob = await _database.InsertUserAsync(Ct);
        await using var context = _database.CreateAppContext();
        await using var transaction = await new EfUserTransactionFactory(context).BeginAsync(alice, Ct);
        context.Garments.Add(NewRecord(GarmentId.New(), bob));

        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            () => new EfUnitOfWork(context).SaveChangesAsync(Ct));

        var postgres = Assert.IsType<PostgresException>(exception.InnerException);
        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, postgres.SqlState);
    }

    [Fact]
    public async Task A_committed_user_does_not_leak_into_the_next_use_of_a_pooled_connection()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var alicesGarment = await _database.InsertGarmentAsync(alice, Ct);

        // A private pool of one connection forces the second context onto the first one's session.
        var connectionString = new NpgsqlConnectionStringBuilder(_database.AppConnectionString)
        {
            MaxPoolSize = 1,
            ApplicationName = $"leak-test-{Guid.NewGuid():N}",
        }.ConnectionString;

        int firstBackend;
        await using (var context = new BucklDbContext(BucklDbContextOptions.Build(connectionString)))
        {
            await using var transaction = await new EfUserTransactionFactory(context).BeginAsync(alice, Ct);
            firstBackend = await BackendProcessIdAsync(context);
            await transaction.CommitAsync(Ct);
        }

        await using var next = new BucklDbContext(BucklDbContextOptions.Build(connectionString));
        await next.Database.OpenConnectionAsync(Ct);

        Assert.Equal(firstBackend, await BackendProcessIdAsync(next));
        Assert.False(await next.Garments.AnyAsync(garment => garment.Id == alicesGarment.Value, Ct));
    }

    private static GarmentRecord NewRecord(GarmentId id, UserId owner) => new()
    {
        Id = id.Value,
        UserId = owner.Value,
        Category = "top",
        Color = "blue",
        Source = "manual",
        Status = "active",
        CreatedAt = TestClock.Now,
        UpdatedAt = TestClock.Now,
    };

    private static async Task<int> BackendProcessIdAsync(BucklDbContext context)
    {
        var connection = context.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = "select pg_backend_pid()";
        command.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();

        return (int)(await command.ExecuteScalarAsync(Ct))!;
    }
}
