using Doniyors.Data;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using telegram_bot.Models;

namespace telegram_bot.Services
{
    /// <summary>
    /// Labels a member's menu button — the one that opens the Mini App — in the
    /// language stored for them. Called whenever the bot sets or confirms that
    /// language; the API does the same for changes made in the Mini App or the
    /// admin panel.
    /// </summary>
    public class MiniAppMenuButton
    {
        private readonly ITelegramBotClient _bot;
        private readonly MiniAppOptions _options;
        private readonly ILogger<MiniAppMenuButton> _logger;

        public MiniAppMenuButton(
            ITelegramBotClient bot,
            IOptions<MiniAppOptions> options,
            ILogger<MiniAppMenuButton> logger)
        {
            _bot = bot;
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>Best effort: a failure is logged and the conversation carries on.</summary>
        public async Task UpdateAsync(
            long chatId,
            string? languageCode,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.Url))
                return;

            try
            {
                if (SupportedLanguages.IsSupported(languageCode))
                {
                    await _bot.SetChatMenuButton(
                        chatId,
                        new MenuButtonWebApp
                        {
                            Text = SupportedLanguages.MenuButtonText(languageCode),
                            WebApp = new WebAppInfo { Url = _options.Url },
                        },
                        cancellationToken);
                }
                else
                {
                    // No language yet: back to the bot-wide default button.
                    await _bot.SetChatMenuButton(chatId, cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(
                    ex,
                    "Could not relabel the menu button. ChatId={ChatId}",
                    chatId);
            }
        }
    }
}
