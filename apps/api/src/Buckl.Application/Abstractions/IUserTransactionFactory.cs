using Buckl.Domain.Users;

namespace Buckl.Application.Abstractions;

/// <summary>Opens the transaction that scopes one request to one user (ADR-0025).</summary>
public interface IUserTransactionFactory
{
    Task<IUserTransaction> BeginAsync(UserId userId, CancellationToken cancellationToken = default);
}
