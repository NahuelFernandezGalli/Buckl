namespace Buckl.Application.Abstractions;

/// <summary>A database transaction bound to one user: Row-Level Security sees that user until
/// the transaction ends. Disposing it without <see cref="CommitAsync"/> rolls every change
/// back.</summary>
public interface IUserTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
