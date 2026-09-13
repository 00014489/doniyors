namespace Doniyors.Data
{
    /// <summary>
    /// The identity fields Telegram gives us about a person, from whichever
    /// side they arrived on — the Mini App's signed <c>initData</c> or a bot
    /// update.
    /// </summary>
    /// <param name="TgUserId">Telegram's numeric id. The account's real key.</param>
    /// <param name="FirstName">Always present on Telegram.</param>
    /// <param name="UserName">The @username. Optional on Telegram; null or empty when unset.</param>
    /// <param name="LanguageCode">The client's locale, e.g. "en". A hint, not a choice.</param>
    /// <param name="PhotoUrl">
    /// Profile picture. Only the Mini App receives this; a bot update carries
    /// no picture, so the bot passes <c>null</c> to mean "unknown", which
    /// leaves any stored picture alone.
    /// </param>
    public readonly record struct TelegramProfile(
        long TgUserId,
        string FirstName,
        string? UserName,
        string? LanguageCode,
        string? PhotoUrl);
}
