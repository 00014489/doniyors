using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using telegram_bot.DAL.Entities;
using telegram_bot.Keyboards;
using telegram_bot.Services;
using telegram_bot.Services.Localization;

namespace telegram_bot.Handlers
{
    public class MessageHandler
    {
        private readonly CommandHandler _commandHandler;
        private readonly UserService _userService;
        private readonly SessionService _sessionService;
        private readonly ReplyKeyboards _replyKeyboards;
        private readonly InlineKeyboards _inlineBtns;
        private readonly ILogger<MessageHandler> _logger;
        private readonly ILocalizationService _localizationService;
        private readonly ITelegramBotClient _bot;

        public MessageHandler(ITelegramBotClient bot, CommandHandler commandHandler, UserService userService, SessionService sessionService, ReplyKeyboards replyKeyboards, InlineKeyboards inlineBtns, ILogger<MessageHandler> logger, ILocalizationService localizationService)
        {
            _bot = bot;
            _commandHandler = commandHandler;
            _userService = userService;
            _sessionService = sessionService;
            _replyKeyboards = replyKeyboards;
            _inlineBtns = inlineBtns;
            _logger = logger;
            _localizationService = localizationService;
        }

        public async Task HandleAsync(Message message)
        {
            switch (message)
            {
                case { Text: not null }:
                    var sessionStep = await _sessionService.GetSessionStepAsync(message.From!.Id);
                    if (sessionStep != SessionStep.None && sessionStep != null)
                    {   
                        switch (sessionStep)
                        {
                            case SessionStep.WaitingForLanguage:

                                if (_localizationService.IsTranslation("cancel", message.Text!))
                                {
                                    await _sessionService.SetSessionStepAsync(message.From!.Id, SessionStep.None);

                                    var user = await _userService.GetUserByTgIdAsync(message.From.Id);
                                    if (user == null)
                                    {
                                        _logger.LogWarning("User not found. TgUserId={TgUserId}", message.From.Id);
                                        return;
                                    }

                                    await _bot.SendMessage(
                                        message.Chat.Id,
                                        _localizationService.Get(user.LanguageCode, "cancel_message"),
                                        replyMarkup: await _replyKeyboards.MainMenu(message.From.Id, user.LanguageCode));

                                    break;
                                }

                                var langCode = _localizationService.GetLanguageCode(message.Text!);

                                if (langCode is not null)
                                {
                                    await _userService.UpdateLanguage(message.From!.Id, langCode);
                                    await _sessionService.SetSessionStepAsync(message.From!.Id, SessionStep.None);

                                    await _bot.SendMessage(
                                        message.Chat.Id,
                                        _localizationService.Get(langCode, "welcome"),
                                        replyMarkup: await _replyKeyboards.MainMenu(message.From.Id, langCode));
                                }
                                else
                                {
                                    await _bot.SendMessage(
                                        message.Chat.Id,
                                        _localizationService.GetAllMessages("another_lang"));
                                }

                                break;
                            default:
                                await _bot.SendMessage(message.Chat.Id, "You are currently in a session. Please complete it before sending other messages.");
                                break;
                        }
                        return;
                    }
                    else if (message.Text.StartsWith("/"))
                    {
                        await _commandHandler.HandleAsync(message);
                    }
                    else if (_localizationService.IsTranslation("profile", message.Text))
                    {
                        var user = await _userService.GetUserByTgIdAsync(message.From!.Id);
                        if (user == null)
                        {
                            _logger.LogWarning("User not found. TgUserId={TgUserId}", message.From!.Id);
                            return;
                        }
                        var lang = user.LanguageCode;

                        var profileMessage =
                            $"👤 <b>{message.From?.FirstName}</b>\n\n" +
                            $"──────────────\n" +
                            // $"👤 <b>{_localizationService.Get(lang, "user_name")}:</b> {user.UserName}\n" +
                            $"🌐 <b>{_localizationService.Get(lang, "language_is")}:</b> {_localizationService.Get(user.LanguageCode, "full_name")}\n" +
                            $"📅 <b>{_localizationService.Get(lang, "registered_at")}:</b> {user.RegisteredAt:dd.MM.yyyy HH:mm}\n\n" +
                            $"⭐ <b>{_localizationService.Get(lang, "points")}:</b> <tg-spoiler>{user.Points}</tg-spoiler>\n" +
                            $"──────────────";
                            await _bot.SendMessage(
                                message.Chat.Id,
                                profileMessage,
                                replyMarkup: _replyKeyboards.ChangeLanguageButton(lang),
                                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    }
                    else if (_localizationService.IsTranslation("main_menu", message.Text))
                    {
                        var user = await _userService.GetUserByTgIdAsync(message.From.Id);
                        if (user == null)
                        {
                            _logger.LogWarning("User not found. TgUserId={TgUserId}", message.From.Id);
                            return;
                        }
                        var lang = user.LanguageCode;

                        await _bot.SendMessage(
                            message.Chat.Id,
                            _localizationService.Get(lang, "returned_main_menu"),
                            replyMarkup: await _replyKeyboards.MainMenu(message.From.Id, lang));
                    }
                    else if (_localizationService.IsTranslation("change_language", message.Text))
                    {
                        var user = await _userService.GetUserByTgIdAsync(message.From.Id);
                        if (user == null)
                        {
                            _logger.LogWarning("User not found. TgUserId={TgUserId}", message.From.Id);
                            return;
                        }
                        await _sessionService.SetSessionStepAsync(message.From.Id, SessionStep.WaitingForLanguage);
                        await _bot.SendMessage(
                            message.Chat.Id,
                            _localizationService.GetAllMessages("choose_language"),
                            replyMarkup: _replyKeyboards.SelectLanguage(_localizationService.GetLanguageFullNames(), user.LanguageCode));
                    }
                    else
                    {
                        await _bot.SendMessage(
                            message.Chat.Id,
                            $"You said: {message.Text}");
                    }
                    break;

                case { Photo: not null }:
                    await _bot.SendMessage(message.Chat.Id, "Nice photo!");
                    break;

                case { Document: not null }:
                    await _bot.SendMessage(message.Chat.Id, "I received your document.");
                    break;

                default:
                    await _bot.SendMessage(message.Chat.Id, "I don't support this message type yet.");
                    break;
            }
        }
    }
}