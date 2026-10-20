using Buckl.Api.Filters;
using Microsoft.Extensions.Logging;

namespace Buckl.Api.Tests.Filters;

/// <summary>The queue behind <c>IAfterCommit</c>, on its own: what it does when work it runs
/// fails.</summary>
public sealed class AfterCommitActionsTests
{
    private readonly CapturingLogger _logger = new();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Actions_run_in_the_order_they_were_queued()
    {
        var order = new List<string>();
        var queue = new AfterCommitActions(_logger);
        queue.Enqueue(_ => Ran(order, "first"));
        queue.Enqueue(_ => Ran(order, "second"));

        await queue.RunAsync(Ct);

        Assert.Equal(["first", "second"], order);
    }

    [Fact]
    public async Task An_action_that_throws_does_not_stop_the_ones_after_it_or_fail_the_run()
    {
        var order = new List<string>();
        var queue = new AfterCommitActions(_logger);
        queue.Enqueue(_ => Ran(order, "before"));
        queue.Enqueue(_ => throw new TimeoutException("users/secret-object-key"));
        queue.Enqueue(_ => Task.FromException(new InvalidOperationException("second failure")));
        queue.Enqueue(_ => Ran(order, "after"));

        await queue.RunAsync(Ct);

        Assert.Equal(["before", "after"], order);
    }

    [Fact]
    public async Task A_failed_action_is_logged_as_a_warning_without_its_message()
    {
        var queue = new AfterCommitActions(_logger);
        queue.Enqueue(_ => throw new TimeoutException("users/secret-object-key"));

        await queue.RunAsync(Ct);

        var entry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Contains("TimeoutException", entry.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("secret-object-key", entry.Message, StringComparison.Ordinal);
        Assert.Null(entry.Exception);
    }

    [Fact]
    public async Task A_queue_that_ran_does_not_run_again()
    {
        var order = new List<string>();
        var queue = new AfterCommitActions(_logger);
        queue.Enqueue(_ => Ran(order, "once"));

        await queue.RunAsync(Ct);
        await queue.RunAsync(Ct);

        Assert.Equal(["once"], order);
    }

    private static Task Ran(List<string> order, string name)
    {
        order.Add(name);

        return Task.CompletedTask;
    }

    private sealed record LogEntry(LogLevel Level, string Message, Exception? Exception);

    private sealed class CapturingLogger : ILogger<AfterCommitActions>
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
    }
}
