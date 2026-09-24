using Application.Exceptions;
using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TaskManager.Api.ExceptionHandling;

/// <summary>
/// Maps known Domain and Application exceptions to HTTP status codes with a ProblemDetails body.
/// Unknown exceptions are left to the default handler, which returns a generic 500.
/// </summary>
internal class ApiExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int? statusCode = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            VersionConflictException or ConcurrencyException => StatusCodes.Status409Conflict,
            _ => null
        };

        if (statusCode is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = statusCode.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Detail = exception.Message
            }
        });
    }
}
