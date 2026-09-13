using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using telegram_bot.Models;

namespace telegram_bot.Infrastructure
{
    /// <summary>
    /// Registers the webhook — and the menu button that opens the Mini App —
    /// with Telegram once the host is up.
    /// <para>
    /// Done as a hosted service rather than inline in <c>Program</c>: an
    /// unreachable Telegram API there blocks startup before Kestrel binds, so
    /// the container never becomes healthy and the failure is invisible.
    /// </para>
    /// </summary>
    public sealed class WebhookRegistration : IHostedService
    {
        private readonly ITelegramBotClient _bot;
        private readonly WebhookOptions _options;
        private readonly MiniAppOptions _miniApp;
        private readonly ILogger<WebhookRegistration> _logger;

        public WebhookRegistration(
            ITelegramBotClient bot,
            IOptions<WebhookOptions> options,
            IOptions<MiniAppOptions> miniApp,
            ILogger<WebhookRegistration> logger)
        {
            _bot = bot;
            _options = options.Value;
            _miniApp = miniApp.Value;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _bot.SetWebhook(
                    url: _options.BaseUrl,
                    secretToken: _options.Secret,
                    allowedUpdates: [UpdateType.Message, UpdateType.CallbackQuery],
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Webhook registered at {Url}.", _options.BaseUrl);
            }
            catch (Exception ex)
            {
                // Log and keep serving: the webhook may already be registered
                // from a previous run, and a restart loop helps nobody.
                _logger.LogError(
                    ex,
                    "Could not register the webhook at {Url}.",
                    _options.BaseUrl);
            }

            await RegisterMenuButtonAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// Points the menu button beside the message box at the Mini App, so
        /// members can open it without a manual BotFather step after a deploy
        /// or a domain change.
        /// </summary>
        private async Task RegisterMenuButtonAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_miniApp.Url))
                return;

            try
            {
                await _bot.SetChatMenuButton(
                    menuButton: new MenuButtonWebApp
                    {
                        Text = _miniApp.MenuButtonText,
                        WebApp = new WebAppInfo { Url = _miniApp.Url },
                    },
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Menu button now opens the Mini App at {Url}.", _miniApp.Url);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Could not set the menu button to the Mini App at {Url}.",
                    _miniApp.Url);
            }
        }
    }
}
