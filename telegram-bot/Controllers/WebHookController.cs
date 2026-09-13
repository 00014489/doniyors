using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using telegram_bot.Handlers;
using telegram_bot.Models;

namespace telegram_bot.Controllers
{
    [ApiController]
    [Route("webhook")]
    public class WebHookController : ControllerBase
    {
        /// <summary>Telegram echoes the configured secret in this header.</summary>
        private const string SecretHeader = "X-Telegram-Bot-Api-Secret-Token";

        private readonly UpdateHandler _handler;
        private readonly WebhookOptions _options;
        private readonly ILogger<WebHookController> _logger;

        public WebHookController(
            UpdateHandler handler,
            IOptions<WebhookOptions> options,
            ILogger<WebHookController> logger)
        {
            _handler = handler;
            _options = options.Value;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post(
            [FromBody] Update update,
            CancellationToken cancellationToken)
        {
            if (!IsFromTelegram())
            {
                _logger.LogWarning(
                    "Rejected a webhook call with a missing or wrong secret token from {RemoteIp}.",
                    HttpContext.Connection.RemoteIpAddress);

                return Unauthorized();
            }

            await _handler.HandleAsync(update, cancellationToken);

            return Ok();
        }

        /// <summary>
        /// Compared in constant time: a plain comparison leaks, through how long
        /// it takes to fail, how much of a guessed secret was correct.
        /// </summary>
        private bool IsFromTelegram()
        {
            if (!Request.Headers.TryGetValue(SecretHeader, out var provided))
                return false;

            return CryptographicEquals(provided.ToString(), _options.Secret);
        }

        private static bool CryptographicEquals(string left, string right)
        {
            var a = System.Text.Encoding.UTF8.GetBytes(left);
            var b = System.Text.Encoding.UTF8.GetBytes(right);

            return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(a, b);
        }
    }
}
