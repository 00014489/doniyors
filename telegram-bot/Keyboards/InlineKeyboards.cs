using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using telegram_bot.Services.Localization;

namespace telegram_bot.Keyboards
{
    public  class InlineKeyboards
    {
        private readonly ILocalizationService _localizationService;

        public InlineKeyboards(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }
        public  InlineKeyboardMarkup MainMenu()
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("📊 Profile", "menu_profile"),
                    InlineKeyboardButton.WithCallbackData("⚙️ Settings", "menu_settings")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("❓ Help", "menu_help")
                }
            });
        }

        public InlineKeyboardMarkup Confirm(string confirmData, string cancelData)
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("✅ Confirm", confirmData),
                    InlineKeyboardButton.WithCallbackData("❌ Cancel", cancelData)
                }
            });
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

    }
}