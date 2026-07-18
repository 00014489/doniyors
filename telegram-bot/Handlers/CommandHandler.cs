using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using telegram_bot.DAL.Entities;
using telegram_bot.Keyboards;
using telegram_bot.Services;
using telegram_bot.Services.Localization;

namespace telegram_bot.Handlers
{
    public class CommandHandler
    {
        private readonly ITelegramBotClient _bot;
        private readonly UserService _userService;
        private readonly SessionService _sessionService;
        private readonly ReplyKeyboards _replyKeyboards;
        private readonly ILocalizationService _localization;

        public CommandHandler(ITelegramBotClient bot, UserService userService, SessionService sessionService, ReplyKeyboards replyKeyboards, ILocalizationService localization)
        {
            _bot = bot;
            _userService = userService;
            _sessionService = sessionService;
            _replyKeyboards = replyKeyboards;
            _localization = localization;
        }

        public async Task HandleAsync(Message message)
        {
            switch (message.Text)
            {
                case "/start":
                    var tgLang = message.From?.LanguageCode ?? "";
                    var user = await _userService.CreateOrUpdateUserAsync(
                        message.From!.Id,
                        message.From.Username ?? "unknown",
                        tgLang
                    );
                    
                    if (!_localization.IsSupported(user.LanguageCode))
                    {
                        await _bot.SendMessage(
                            message.Chat.Id,
                            _localization.GetAllMessages("choose_language"),
                            replyMarkup: _replyKeyboards.SelectLanguage(_localization.GetLanguageFullNames())
                        );
                        await _sessionService.SetSessionStepAsync(message.From.Id, SessionStep.WaitingForLanguage);
                        return;
                    }
                    await _bot.SendMessage(
                        message.Chat.Id,
                        _localization.Get(user.LanguageCode, "welcome"),
                        replyMarkup: await _replyKeyboards.MainMenu(message.From.Id, user.LanguageCode)
                    );

                    break;
                    

                case "/help":
                    await _bot.SendMessage(
                        message.Chat.Id,
                        "Available commands: /start, /help");
                    break;
                
                default:
                    await _bot.SendMessage(
                        message.Chat.Id,
                        "Unknown command. Type /help to see available commands.");
                    break;
            }
        }
    }
}