using Buckl.Application.Abstractions;

namespace Buckl.Api.Filters;

/// <summary>The request's queue of <see cref="IAfterCommit"/> work. <c>UserTransactionFilter</c>
/// runs it once the transaction has committed and drops it otherwise.</summary>
public sealed partial class AfterCommitActions : IAfterCommit
{
    private readonly List<Func<CancellationToken, Task>> _actions = [];

    private readonly ILogger<AfterCommitActions> _logger;

    public AfterCommitActions(ILogger<AfterCommitActions> logger)
    {
        _logger = logger;
    }

    public void Enqueue(Func<CancellationToken, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        _actions.Add(action);
    }

    /// <summary>Runs the queued actions in the order they were queued. The request has already
    /// committed, so no action can turn it into an error: one that throws is logged and the next one
    /// still runs. A queue never runs twice.</summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var actions = _actions.ToArray();
        _actions.Clear();

        foreach (var action in actions)
        {
            try
            {
                await action(cancellationToken);
            }
            // Deliberately broad: what an action throws is unknowable from here, and it must not undo a
            // commit. Only failures the process cannot recover from are left to propagate.
            catch (Exception exception) when (exception is not OutOfMemoryException)
            {
                // The exception is logged, never its message text: it can carry an object key.
                LogActionFailed(_logger, exception.GetType().Name);
            }
        }
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Follow-up work of a committed request failed and was skipped ({ExceptionType}).")]
    private static partial void LogActionFailed(ILogger logger, string exceptionType);
}
