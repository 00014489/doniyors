namespace telegram_bot.Services.Localization
{
    public interface ILocalizationService
    {
        /// <summary>The text in <paramref name="lang"/>, or in the default language when it has none.</summary>
        string Get(string? lang, string key);

        /// <summary>
        /// For a member with a language: the text in it. For one without (an empty
        /// or unsupported code): the text in every language, so they can read it
        /// before they have chosen.
        /// </summary>
        string Prompt(string? lang, string key);

        bool IsSupported(string? lang);

        List<string> GetSupportedLanguages();

        Dictionary<string, string> GetLanguageFullNames();

        /// <summary>Every language's text for <paramref name="key"/>, one per line.</summary>
        string GetAllMessages(string key);

        string? GetLanguageCode(string fullName);

        bool IsTranslation(string key, string text);
    }
}
