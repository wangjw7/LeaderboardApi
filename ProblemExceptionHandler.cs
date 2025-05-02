using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LeaderboardApi
{
    [Serializable]
    public class ProblemException : Exception
    {
        public string Message { get; set; }

        public string Error { get; set; }
        public ProblemException(string message, string error)
        {
            Error = error;
            Message = message;
        }
    }

    public class ProblemExceptionHandler : IExceptionHandler
    { 
        private readonly IProblemDetailsService _problemDetailsService;

        public ProblemExceptionHandler(IProblemDetailsService problemDetailsService)
        {
            _problemDetailsService = problemDetailsService;
        }

        // note(wangjw): Custom exception-handling logic
        public async ValueTask<bool> TryHandleAsync(
            HttpContext HttpContext, 
            Exception Exception, 
            CancellationToken CancellationToken)
        {
            if (Exception is not ProblemException problemException)
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

            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return await _problemDetailsService.TryWriteAsync(
                new ProblemDetailsContext
                { 
                    HttpContext = HttpContext,
                    ProblemDetails = ProblemDetail
                });
        }
    }
}
