using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LeaderboardApi;

[Serializable]
public class ProblemException(string message, string error) : Exception
{
    public string Message { get; set; } = message;

    public string Error { get; set; } = error;
}

public class ProblemExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;

    // note(wangjw): Custom exception-handling
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {

        if (exception is not ProblemException problemException)
        {
            return true;
        }
        var ProblemDetail = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = problemException.Error,
            Detail = problemException.Message,
            Type = "Bad Request"
        };

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = ProblemDetail
        });
    }
}

