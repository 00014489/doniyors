using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace telegram_bot.Services.Localization
{
    public class LocalizationService: ILocalizationService
    {
        private readonly Dictionary<string, Dictionary<string, string>> _data;

        public LocalizationService(IWebHostEnvironment env)
        {
            var path = Path.Combine(env.ContentRootPath, "language.json");
            var json = File.ReadAllText(path);

            _data = JsonSerializer.Deserialize<
                Dictionary<string, Dictionary<string, string>>
            >(json)!;
        }

        public string Get(string lang, string key)
        {
            if (_data.TryGetValue(lang, out var dict) &&
                dict.TryGetValue(key, out var value))
            {
                return value;
            }

            return _data["en"][key]; // fallback
        }

        public bool IsSupported(string lang)
            => _data.ContainsKey(lang);

        public List<string> GetSupportedLanguages()
            => _data.Keys.ToList();

        public Dictionary<string, string> GetLanguageFullNames()
        {
            var result = new Dictionary<string, string>();

            foreach (var lang in _data.Keys)
            {
                if (_data[lang].TryGetValue("full_name", out var name))
                {
                    result[lang] = name;
                }
            }

            return result;
        }
        public string GetAllMessages(string lang)
        {
            return string.Join(
                Environment.NewLine,
                _data.Values
                    .Where(x => x.ContainsKey(lang))
                    .Select(x => x[lang]));
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
                .FirstOrDefault(x =>
                    x.Value.TryGetValue("full_name", out var name) &&
                    name == fullName)
                .Key;
        }
    }
}