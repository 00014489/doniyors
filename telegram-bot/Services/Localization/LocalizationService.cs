using System.Text.Json;
using Doniyors.Data;

namespace telegram_bot.Services.Localization
{
    public class LocalizationService : ILocalizationService
    {
        private readonly Dictionary<string, Dictionary<string, string>> _data;

        public LocalizationService(IWebHostEnvironment env)
        {
            var path = Path.Combine(env.ContentRootPath, "language.json");
            var json = File.ReadAllText(path);

            _data = JsonSerializer.Deserialize<
                Dictionary<string, Dictionary<string, string>>
            >(json)!;

            // Users.LanguageCode may hold any supported language. A section
            // missing here would quietly answer those members in English, so it
            // stops the bot at startup instead.
            var missing = SupportedLanguages.All
                .Select(language => language.Code)
                .Where(code => !_data.ContainsKey(code))
                .ToList();

            if (missing.Count > 0)
            {
                throw new InvalidOperationException(
                    $"language.json has no section for: {string.Join(", ", missing)}.");
            }
        }

        public string Get(string? lang, string key)
        {
            if (lang is not null
                && _data.TryGetValue(lang, out var dict)
                && dict.TryGetValue(key, out var value))
            {
                return value;
            }

            return _data[SupportedLanguages.Default][key];
        }

        public string Prompt(string? lang, string key) =>
            IsSupported(lang) ? Get(lang, key) : GetAllMessages(key);

        public bool IsSupported(string? lang) =>
            SupportedLanguages.IsSupported(lang) && _data.ContainsKey(lang!);

        public List<string> GetSupportedLanguages() =>
            _data.Keys.Where(IsSupported).ToList();

        public Dictionary<string, string> GetLanguageFullNames()
        {
            var result = new Dictionary<string, string>();

            foreach (var lang in _data.Keys.Where(IsSupported))
            {
                if (_data[lang].TryGetValue("full_name", out var name))
                {
                    result[lang] = name;
                }
            }

            return result;
        }

        public string GetAllMessages(string key)
        {
            return string.Join(
                Environment.NewLine,
                _data
                    .Where(x => IsSupported(x.Key) && x.Value.ContainsKey(key))
                    .Select(x => x.Value[key]));
        }

        public bool IsTranslation(string key, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return _data.Values
                .Where(lang => lang.ContainsKey(key))
                .Select(lang => lang[key])
                .Any(value => value.Equals(text, StringComparison.OrdinalIgnoreCase));
        }

        public string? GetLanguageCode(string fullName)
        {
            return _data
                .Where(x => IsSupported(x.Key))
                .FirstOrDefault(x =>
                    x.Value.TryGetValue("full_name", out var name) &&
                    name == fullName)
                .Key;
        }
    }
}
