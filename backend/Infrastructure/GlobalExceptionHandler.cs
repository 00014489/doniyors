using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace backend.Infrastructure
{
    /// <summary>
    /// Turns escaping exceptions into RFC 9457 problem responses. Without it a
    /// rejected Telegram login surfaced as a 500, which told the client to
    /// retry rather than to reopen the Mini App.
    /// </summary>
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            IProblemDetailsService problemDetailsService,
            ILogger<GlobalExceptionHandler> logger)
        {
            _problemDetailsService = problemDetailsService;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (status, title) = Map(exception);

            // Expected rejections are noise at Error level; unexpected ones are not.
            if (status >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception for {Method} {Path}.",
                    httpContext.Request.Method,
                    httpContext.Request.Path);
            }
            else
            {
                _logger.LogInformation(
                    "Request rejected for {Method} {Path}: {Message}",
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    exception.Message);
            }

            httpContext.Response.StatusCode = status;

            return await _problemDetailsService.TryWriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    Exception = exception,
                    ProblemDetails = new ProblemDetails
                    {
                        Status = status,
                        Title = title,
                        // The message of an expected rejection is written for
                        // the user; an unexpected one may leak internals.
                        Detail = status >= StatusCodes.Status500InternalServerError
                            ? null
                            : exception.Message,
                    },
                });
        }

        private static (int Status, string Title) Map(Exception exception) =>
            exception switch
            {
                UnauthorizedAccessException =>
                    (StatusCodes.Status401Unauthorized, "Unauthorized"),

                ArgumentException or FormatException =>
                    (StatusCodes.Status400BadRequest, "Bad Request"),

                OperationCanceledException =>
                    (StatusCodes.Status499ClientClosedRequest, "Client Closed Request"),

                _ =>
                    (StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
            };
    }
}
