using Buckl.Application.Common;
using Buckl.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Buckl.Api.Errors;

/// <summary>Maps the exceptions that carry a code to problem details (ADR-0015): invalid input to
/// 400, resources the user cannot see to 404, rules about the current state to 409. Any other
/// exception is left to the default handler, which answers 500 without details.</summary>
public sealed class DomainExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;

    public DomainExceptionHandler(IProblemDetailsService problemDetails)
    {
        _problemDetails = problemDetails;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (Map(exception) is not { } problem)
        {
            return false;
        }

        httpContext.Response.StatusCode = problem.Status;

        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = problem.Status,
                Title = problem.Title,
                Detail = exception.Message,
                Extensions = { [ProblemCodes.Key] = problem.Code },
            },
        });
    }

    private static (int Status, string Title, string Code)? Map(Exception exception) => exception switch
    {
        ResourceNotFoundException notFound =>
            (StatusCodes.Status404NotFound, "Resource not found", notFound.Code),
        DomainValidationException invalid =>
            (StatusCodes.Status400BadRequest, "Invalid request", invalid.Code),
        DomainException conflict =>
            (StatusCodes.Status409Conflict, "Conflict with the current state", conflict.Code),
        _ => null,
    };
}
