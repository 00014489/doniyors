namespace Doniyors.Data
{
    /// <summary>A language the product is translated into.</summary>
    /// <param name="Code">The value stored in <c>Users.LanguageCode</c>, e.g. "uz".</param>
    /// <param name="MenuButtonText">Label of the bot's menu button that opens the Mini App.</param>
    public sealed record SupportedLanguage(string Code, string MenuButtonText);

    /// <summary>
    /// The values <c>Users.LanguageCode</c> may hold.
    /// <para>
    /// That column is the single source of truth for a member's language: the
    /// bot, the Mini App and the admin panel all read it, and only these codes —
    /// or the empty string, meaning "not chosen yet" — are ever written to it.
    /// The bot's language.json and the Angular app's i18n files carry one
    /// section per code here.
    /// </para>
    /// </summary>
    public static class SupportedLanguages
    {
        public const string Uzbek = "uz";
        public const string Russian = "ru";
        public const string English = "en";

        /// <summary>What every part of the product shows a member who has no language yet.</summary>
        public const string Default = English;

        public static IReadOnlyList<SupportedLanguage> All { get; } =
        [
            new(Uzbek, "Ilovani ochish"),
            new(Russian, "Открыть приложение"),
            new(English, "Open app"),
        ];

        public static bool IsSupported(string? code) =>
            code is not null && All.Any(language => language.Code == code);

        /// <summary>
        /// The supported code for a language tag such as Telegram's "ru", "en-US"
        /// or "pt-br"; <c>null</c> when the product does not speak that language.
        /// </summary>
        public static string? Normalize(string? tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return null;

            var primary = tag.Trim().Split('-', '_')[0].ToLowerInvariant();

            return IsSupported(primary) ? primary : null;
        }

        /// <summary>The menu button label in a language, or in the default one.</summary>
        public static string MenuButtonText(string? code) =>
            (All.FirstOrDefault(language => language.Code == code)
                ?? All.First(language => language.Code == Default)).MenuButtonText;
    }
}
