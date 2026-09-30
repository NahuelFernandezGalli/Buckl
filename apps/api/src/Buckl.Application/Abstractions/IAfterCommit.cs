namespace Buckl.Application.Abstractions;

/// <summary>Follow-up work of a use case that must not happen unless the request's database
/// transaction commits, such as deleting a photo a saved change no longer uses. Queued actions run
/// after the commit and never after a rollback, so a failed request cannot leave a garment pointing
/// at something already deleted.</summary>
public interface IAfterCommit
{
    void Enqueue(Func<CancellationToken, Task> action);
}
