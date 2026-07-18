using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace telegram_bot.Services.Localization
{
    public interface ILocalizationService
    {
        string Get(string lang, string key);
        bool IsSupported(string lang);
        List<string> GetSupportedLanguages();
        Dictionary<string, string> GetLanguageFullNames();
        string GetAllMessages(string lang);
        string? GetLanguageCode(string fullName);
        bool IsTranslation(string key, string text);
    }
}