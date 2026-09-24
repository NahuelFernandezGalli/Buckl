using Buckl.Application.Abstractions;
using Buckl.Domain.Users;
using Buckl.Infrastructure.Persistence;

namespace Buckl.Testing;

/// <summary>An application-role context inside a transaction bound to one user, exactly as a
/// request sees the database. Dispose without <see cref="SaveAndCommitAsync"/> to roll
/// back.</summary>
public sealed class UserScope : IAsyncDisposable
{
    private readonly IUserTransaction _transaction;

    private UserScope(BucklDbContext context, IUserTransaction transaction)
    {
        Context = context;
        _transaction = transaction;
    }

    public BucklDbContext Context { get; }

    public static async Task<UserScope> BeginAsync(
        PostgresDatabase database,
        UserId userId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(database);

        var context = database.CreateAppContext();

        try
        {
            var transaction = await new EfUserTransactionFactory(context).BeginAsync(userId, cancellationToken);
            return new UserScope(context, transaction);
        }
        catch
        {
            await context.DisposeAsync();
            throw;
        }
    }

    public async Task SaveAndCommitAsync(CancellationToken cancellationToken)
    {
        await new EfUnitOfWork(Context).SaveChangesAsync(cancellationToken);
        await _transaction.CommitAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _transaction.DisposeAsync();
        await Context.DisposeAsync();
    }
}
