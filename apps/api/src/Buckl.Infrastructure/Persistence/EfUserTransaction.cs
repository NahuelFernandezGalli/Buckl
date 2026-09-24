using Buckl.Application.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Buckl.Infrastructure.Persistence;

internal sealed class EfUserTransaction : IUserTransaction
{
    private readonly IDbContextTransaction _transaction;

    public EfUserTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default) =>
        _transaction.CommitAsync(cancellationToken);

    public ValueTask DisposeAsync() => _transaction.DisposeAsync();
}
