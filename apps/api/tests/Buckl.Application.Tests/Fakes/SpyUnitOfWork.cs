using Buckl.Application.Abstractions;

namespace Buckl.Application.Tests.Fakes;

internal sealed class SpyUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;

        return Task.CompletedTask;
    }
}
