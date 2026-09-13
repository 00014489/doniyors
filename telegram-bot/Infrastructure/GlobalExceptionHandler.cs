using Microsoft.AspNetCore.Diagnostics;

namespace telegram_bot.Infrastructure
{
    /// <summary>
    /// Replaces the hand-rolled middleware, which wrote the message to the
    /// console (losing the stack trace) and answered 500.
    /// <para>
    /// Telegram retries a webhook it could not deliver, so a 500 on a bug that
    /// will fail again turns one broken update into a retry loop that blocks
    /// every later update for that chat. The failure is logged in full and
    /// acknowledged with 200 instead.
    /// </para>
    /// </summary>
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}.",
                httpContext.Request.Method,
                httpContext.Request.Path);

            httpContext.Response.StatusCode = StatusCodes.Status200OK;

            return ValueTask.FromResult(true);
        }
    }
}
