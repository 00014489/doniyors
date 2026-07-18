using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using telegram_bot.Keyboards;
using telegram_bot.Services;
using telegram_bot.Services.Localization;

namespace telegram_bot.Handlers
{
    public class CallbackQueryHandler
    {
        private readonly ITelegramBotClient _bot;
        private readonly InlineKeyboards _inlineBtns;
        private readonly ReplyKeyboards _replyKeyboards;
        private readonly UserService _userService;
        private readonly ILocalizationService _localization;

        public CallbackQueryHandler(
            ITelegramBotClient bot,
            InlineKeyboards inlineBtns,
            ReplyKeyboards replyKeyboards,
            UserService userService,
            ILocalizationService localization)
        {
            _bot = bot;
            _inlineBtns = inlineBtns;
            _replyKeyboards = replyKeyboards;
            _userService = userService;
            _localization = localization;
        }

        public async Task HandleAsync(CallbackQuery callback)
        {
            switch (callback.Data)
            {
                case "change_language":
                    var user = await _userService.GetUserByTgIdAsync(callback.From.Id);

                    await _bot.EditMessageReplyMarkup(
                        callback.Message!.Chat.Id,
                        callback.Message.MessageId,
                        $"{_localization.Get(user!.LanguageCode, "choose_language")}");
                        // replyMarkup: _replyKeyboards.SelectLanguage(_localization.GetLanguageFullNames()));

                    // await _bot.AnswerCallbackQuery(callback.Id);

                    break;
            }
        }
    }
}