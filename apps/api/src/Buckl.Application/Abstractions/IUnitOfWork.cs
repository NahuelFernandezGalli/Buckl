namespace Buckl.Application.Abstractions;

/// <summary>Persists every change the repositories staged during the current request. Handlers
/// call it once, after the domain accepted the change. The surrounding transaction belongs to the
/// request and is committed by the API, not by the handler (ADR-0025).</summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
