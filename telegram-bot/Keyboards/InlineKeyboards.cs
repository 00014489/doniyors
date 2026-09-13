using System.Globalization;
using Telegram.Bot.Types.ReplyMarkups;
using telegram_bot.Models;
using telegram_bot.Services.Localization;

namespace telegram_bot.Keyboards
{
    public class InlineKeyboards
    {
        /// <summary>Callback data of an adventure in the QR scan picker: prefix + travel id.</summary>
        private const string ScanAdventurePrefix = "scan_adventure:";

        private readonly ILocalizationService _localizationService;

        public InlineKeyboards(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public InlineKeyboardMarkup ChangeLanguageButton(string langCode)
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        _localizationService.Get(langCode, "change_language"),
                        "change_language")
                }
            });
        }

        /// <summary>One button per adventure, showing its date, title and reward.</summary>
        public InlineKeyboardMarkup ScanAdventures(IEnumerable<ScanAdventure> adventures)
        {
            return new InlineKeyboardMarkup(
                adventures.Select(adventure => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"{adventure.TravelDate.ToLocalTime():dd.MM.yyyy} · {adventure.Title} · +{adventure.Points}",
                        $"{ScanAdventurePrefix}{adventure.Id}")
                }));
        }

        /// <summary>Recognises a tap on <see cref="ScanAdventures"/> and reads the travel id from it.</summary>
        public static bool TryParseScanAdventure(string? callbackData, out int travelId)
        {
            travelId = 0;

            return callbackData is not null
                && callbackData.StartsWith(ScanAdventurePrefix, StringComparison.Ordinal)
                && int.TryParse(
                    callbackData.AsSpan(ScanAdventurePrefix.Length),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out travelId);
        }
    }
}
