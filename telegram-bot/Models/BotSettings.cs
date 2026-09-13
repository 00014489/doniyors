using System.ComponentModel.DataAnnotations;

namespace telegram_bot.Models
{
    /// <summary>
    /// Bound from the "Telegram" section — the same section name and key the
    /// API uses, so one <c>Telegram__BotToken</c> variable configures both.
    /// </summary>
    public sealed class TelegramOptions
    {
        public const string SectionName = "Telegram";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Telegram:BotToken is required.")]
        public string BotToken { get; set; } = string.Empty;
    }

    /// <summary>Bound from the "Webhook" section.</summary>
    public sealed class WebhookOptions
    {
        public const string SectionName = "Webhook";

        /// <summary>Public HTTPS URL Telegram posts updates to.</summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Webhook:BaseUrl is required.")]
        [Url]
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Echoed back by Telegram in the <c>X-Telegram-Bot-Api-Secret-Token</c>
        /// header and checked on every update. Without it the webhook endpoint
        /// accepts forged updates from anyone who learns the URL.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Webhook:Secret is required.")]
        [StringLength(256, MinimumLength = 1)]
        [RegularExpression(
            "^[A-Za-z0-9_-]+$",
            ErrorMessage = "Webhook:Secret may only contain A-Z, a-z, 0-9, _ and -.")]
        public string Secret { get; set; } = string.Empty;
    }

    /// <summary>
    /// Bound from the "MiniApp" section. Optional: when <see cref="Url"/> is
    /// set, the bot's menu button opens the Mini App at that address.
    /// </summary>
    public sealed class MiniAppOptions
    {
        public const string SectionName = "MiniApp";

        /// <summary>Public HTTPS address of the Angular app.</summary>
        [Url(ErrorMessage = "MiniApp:Url must be an absolute URL.")]
        public string? Url { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string MenuButtonText { get; set; } = "Open app";
    }
}
