using System.ComponentModel.DataAnnotations;

namespace backend.Options
{
    /// <summary>
    /// Bound from the "Telegram" section. In containers this arrives as the
    /// <c>Telegram__BotToken</c> environment variable.
    /// </summary>
    public sealed class TelegramOptions
    {
        public const string SectionName = "Telegram";

        /// <summary>Used to verify the HMAC signature on Mini App initData.</summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Telegram:BotToken is required.")]
        public string BotToken { get; set; } = string.Empty;
    }
}
