using Buckl.Api.Authentication;
using Buckl.Application.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Buckl.Api.Filters;

/// <summary>Binds every controller action to its user (ADR-0025): resolves the local user from the
/// <c>sub</c> claim, binds <see cref="ICurrentUser"/>, and runs the action inside a transaction in
/// which Row-Level Security sees that user. The transaction commits only if the action completed
/// without an exception; otherwise disposing it rolls everything back. Work queued through
/// <see cref="Buckl.Application.Abstractions.IAfterCommit"/> runs after the commit, never after a
/// rollback.</summary>
public sealed class UserTransactionFilter : IAsyncActionFilter
{
    private readonly IUserProvisioning _provisioning;

    private readonly IUserTransactionFactory _transactions;

    private readonly RequestUser _requestUser;

    private readonly AfterCommitActions _afterCommit;

    public UserTransactionFilter(
        IUserProvisioning provisioning,
        IUserTransactionFactory transactions,
        RequestUser requestUser,
        AfterCommitActions afterCommit)
    {
        _provisioning = provisioning;
        _transactions = transactions;
        _requestUser = requestUser;
        _afterCommit = afterCommit;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var subject = context.HttpContext.User.FindFirst(BucklClaims.Subject)?.Value;

        if (subject is null)
        {
            // Anonymous actions (none today) run without a user and see no user-scoped rows.
            await next();
            return;
        }

        var cancellationToken = context.HttpContext.RequestAborted;
        var userId = await _provisioning.EnsureUserAsync(subject, cancellationToken);
        _requestUser.Bind(userId);

        await using var transaction = await _transactions.BeginAsync(userId, cancellationToken);
        var executed = await next();

        if (executed.Exception is null || executed.ExceptionHandled)
        {
            await transaction.CommitAsync(cancellationToken);

            // Not cancellable by the client leaving: the change is committed and its follow-up
            // work has to finish.
            await _afterCommit.RunAsync(CancellationToken.None);
        }
    }
}
