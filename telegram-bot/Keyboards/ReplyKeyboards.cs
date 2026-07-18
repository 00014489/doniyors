using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using telegram_bot.Services;
using telegram_bot.Services.Localization;

namespace telegram_bot.Keyboards
{
    public class ReplyKeyboards
    {
        private readonly UserService _userService;
        private readonly ILocalizationService _localizationService;

        public ReplyKeyboards(UserService userService, ILocalizationService localizationService)
        {
            _userService = userService;
            _localizationService = localizationService;
        }

        public async Task<ReplyKeyboardMarkup> MainMenu(long userId, string langCode)
        {

            var buttons = new List<KeyboardButton[]>();

            if (await _userService.IsAdminAsync(userId))
            {
                buttons.Add(new[]
                {
                    new KeyboardButton(_localizationService.Get(langCode, "scan_qr")),
                    new KeyboardButton(_localizationService.Get(langCode, "profile"))
                });
            }
            else
            {
                buttons.Add(new[]
                {
                    new KeyboardButton(_localizationService.Get(langCode, "profile"))
                });
            }

            return new ReplyKeyboardMarkup(buttons)
            {
                ResizeKeyboard = true
            };
        }

        public static ReplyKeyboardRemove Remove()
        {
            return new ReplyKeyboardRemove();
        }

        public ReplyKeyboardMarkup SelectLanguage(Dictionary<string, string> fullNames, string? langCode = null)
        {
            var rows = fullNames
                .Select(kvp => new KeyboardButton[]
                {
                    new KeyboardButton(kvp.Value) // kvp.Value = full_name, kvp.Key = lang code
                })
                .ToArray();

            if (langCode is not null)
            {
                var backButton = new KeyboardButton(_localizationService.Get(langCode!, "cancel"));
                rows = rows.Append(new[] { backButton }).ToArray();
            }

            return new ReplyKeyboardMarkup(rows)
            {
                ResizeKeyboard = true
            };
        }
        
        public ReplyKeyboardMarkup ChangeLanguageButton(string langCode)
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new[]
                {
                    new KeyboardButton(
                        _localizationService.Get(langCode, "change_language"))
                },
                new[]
                {
                    new KeyboardButton(
                        _localizationService.Get(langCode, "main_menu"))
                }
            })
            {
                ResizeKeyboard = true
            };
        }
    }
}