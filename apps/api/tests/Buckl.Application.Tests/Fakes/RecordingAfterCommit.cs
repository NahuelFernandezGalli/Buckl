using Buckl.Application.Abstractions;

namespace Buckl.Application.Tests.Fakes;

/// <summary>Collects the follow-up work a handler queues, so a test decides when the request's
/// transaction "commits" by calling <see cref="RunAllAsync"/>.</summary>
internal sealed class RecordingAfterCommit : IAfterCommit
{
    private readonly List<Func<CancellationToken, Task>> _actions = [];

    public int Pending => _actions.Count;

    public void Enqueue(Func<CancellationToken, Task> action) => _actions.Add(action);

    public async Task RunAllAsync()
    {
        foreach (var action in _actions)
        {
            await action(CancellationToken.None);
        }

        _actions.Clear();
    }
}
