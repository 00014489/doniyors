using System.Net.Http.Json;
using backend.Options;
using Doniyors.Data;
using Microsoft.Extensions.Options;

namespace backend.Services.TelegramMenuButton
{
    public interface ITelegramMenuButton
    {
        /// <summary>Relabels the member's menu button in <paramref name="languageCode"/>. Never throws.</summary>
        Task UpdateAsync(
            long tgUserId,
            string languageCode,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Keeps the bot's menu button — the one that opens the Mini App — labelled
    /// in the member's language when the API changes that language (the Mini
    /// App's picker, the admin panel). The bot does the same for changes made
    /// in the bot.
    /// <para>
    /// Best effort: someone who never started the bot has no chat to relabel,
    /// and a Telegram outage must not fail a language change that is already
    /// saved. Failures are logged, never thrown.
    /// </para>
    /// </summary>
    public sealed class TelegramMenuButton : ITelegramMenuButton
    {
        private readonly HttpClient _http;
        private readonly TelegramOptions _telegram;
        private readonly MiniAppOptions _miniApp;
        private readonly ILogger<TelegramMenuButton> _logger;

        public TelegramMenuButton(
            HttpClient http,
            IOptions<TelegramOptions> telegram,
            IOptions<MiniAppOptions> miniApp,
            ILogger<TelegramMenuButton> logger)
        {
            _http = http;
            _telegram = telegram.Value;
            _miniApp = miniApp.Value;
            _logger = logger;
        }

        public async Task UpdateAsync(
            long tgUserId,
            string languageCode,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_miniApp.Url))
                return;

            // No language yet: leaving out menu_button resets the chat to the
            // bot's default button.
            object payload = SupportedLanguages.IsSupported(languageCode)
                ? new
                {
                    chat_id = tgUserId,
                    menu_button = new
                    {
                        type = "web_app",
                        text = SupportedLanguages.MenuButtonText(languageCode),
                        web_app = new { url = _miniApp.Url },
                    },
                }
                : new { chat_id = tgUserId };

            try
            {
                // The bot token is part of this path: never log the request URI.
                // "./" keeps it relative: a token contains a colon, so without
                // it "bot123:ABC/…" parses as an absolute URI whose scheme is
                // "bot123".
                using var response = await _http.PostAsJsonAsync(
                    $"./bot{_telegram.BotToken}/setChatMenuButton",
                    payload,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);

                    _logger.LogWarning(
                        "Telegram refused to relabel the menu button. TgUserId={TgUserId} Status={Status} Response={Response}",
                        tgUserId,
                        (int)response.StatusCode,
                        body);
                }
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                // Anything at all — the language is already saved, and a label
                // must never turn that into a failed request. The exception is
                // not logged whole: its message can quote the request URI, and
                // with it the token.
                _logger.LogWarning(
                    "Could not relabel the menu button. TgUserId={TgUserId} Error={ErrorType}: {Error}",
                    tgUserId,
                    ex.GetType().Name,
                    ex.Message.Replace(_telegram.BotToken, "<token>", StringComparison.Ordinal));
            }
        }
    }
}
