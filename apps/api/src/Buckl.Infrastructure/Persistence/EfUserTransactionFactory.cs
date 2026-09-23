using System.Globalization;
using Buckl.Application.Abstractions;
using Buckl.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Buckl.Infrastructure.Persistence;

/// <summary>Opens a transaction on the request's context and binds it to one user with
/// <c>set_config('app.user_id', …, true)</c>, the parameterizable form of <c>SET LOCAL</c>: the
/// value dies with the transaction, so a pooled connection never carries a user into the next
/// request (ADR-0025).</summary>
public sealed class EfUserTransactionFactory : IUserTransactionFactory
{
    private readonly BucklDbContext _context;

    public EfUserTransactionFactory(BucklDbContext context)
    {
        _context = context;
    }

    public async Task<IUserTransaction> BeginAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var user = userId.Value.ToString("D", CultureInfo.InvariantCulture);
            await _context.Database.ExecuteSqlAsync(
                $"select set_config('app.user_id', {user}, true)",
                cancellationToken);

            return new EfUserTransaction(transaction);
        }
        catch
        {
            await transaction.DisposeAsync();
            throw;
        }
    }
}
