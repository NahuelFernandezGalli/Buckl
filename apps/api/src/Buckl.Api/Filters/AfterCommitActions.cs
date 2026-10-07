using Buckl.Application.Abstractions;

namespace Buckl.Api.Filters;

/// <summary>The request's queue of <see cref="IAfterCommit"/> work. <c>UserTransactionFilter</c>
/// runs it once the transaction has committed and drops it otherwise.</summary>
public sealed class AfterCommitActions : IAfterCommit
{
    private readonly List<Func<CancellationToken, Task>> _actions = [];

    public void Enqueue(Func<CancellationToken, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        _actions.Add(action);
    }

    /// <summary>Runs the queued actions in the order they were queued.</summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        foreach (var action in _actions)
        {
            await action(cancellationToken);
        }

        _actions.Clear();
    }
}
