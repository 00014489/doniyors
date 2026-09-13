using Telegram.Bot;
using Telegram.Bot.Types;
using Doniyors.Data;
using Doniyors.Data.Entities;
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
        private readonly MiniAppMenuButton _menuButton;

        public CommandHandler(
            ITelegramBotClient bot,
            UserService userService,
            SessionService sessionService,
            ReplyKeyboards replyKeyboards,
            ILocalizationService localization,
            MiniAppMenuButton menuButton)
        {
            _bot = bot;
            _userService = userService;
            _sessionService = sessionService;
            _replyKeyboards = replyKeyboards;
            _localization = localization;
            _menuButton = menuButton;
        }

        public async Task HandleAsync(
            Message message,
            CancellationToken cancellationToken = default)
        {
            switch (CommandName(message.Text))
            {
                case "/start":
                    await StartAsync(message, cancellationToken);
                    break;

                case "/help":
                    await ReplyAsync(message, "help", cancellationToken);
                    break;

                default:
                    await ReplyAsync(message, "unknown_command", cancellationToken);
                    break;
            }
        }

        private async Task StartAsync(Message message, CancellationToken cancellationToken)
        {
            // A bot update carries no profile picture, so PhotoUrl is null —
            // meaning "unknown", which leaves any picture the Mini App already
            // stored untouched.
            var profile = new TelegramProfile(
                TgUserId: message.From!.Id,
                FirstName: message.From.FirstName,
                UserName: message.From.Username,
                LanguageCode: message.From.LanguageCode,
                PhotoUrl: null);

            var user = await _userService.CreateOrUpdateUserAsync(profile, cancellationToken);

            if (!_localization.IsSupported(user.LanguageCode))
            {
                // No language stored yet, so the question is asked in all of them.
                await _bot.SendMessage(
                    message.Chat.Id,
                    _localization.Prompt(null, "choose_language"),
                    replyMarkup: _replyKeyboards.SelectLanguage(_localization.GetLanguageFullNames()),
                    cancellationToken: cancellationToken);

                await _sessionService.SetSessionStepAsync(message.From.Id, SessionStep.WaitingForLanguage, cancellationToken);

                return;
            }

            // The language may have been changed in the Mini App or the admin
            // panel since this chat's button was last labelled.
            await _menuButton.UpdateAsync(message.Chat.Id, user.LanguageCode, cancellationToken);

            await _bot.SendMessage(
                message.Chat.Id,
                _localization.Get(user.LanguageCode, "welcome"),
                replyMarkup: await _replyKeyboards.MainMenu(message.From.Id, user.LanguageCode, cancellationToken),
                cancellationToken: cancellationToken);
        }

        private async Task ReplyAsync(Message message, string key, CancellationToken cancellationToken)
        {
            var user = await _userService.GetUserByTgIdAsync(message.From!.Id, cancellationToken);

            await _bot.SendMessage(
                message.Chat.Id,
                _localization.Prompt(user?.LanguageCode, key),
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// "/start" from "/start", "/start payload" (a deep link) or
        /// "/start@SomeBot" (a command addressed to this bot in a group).
        /// </summary>
        private static string CommandName(string? text)
        {
            var first = (text ?? string.Empty).Split(' ', 2)[0];
            var at = first.IndexOf('@');

            return (at >= 0 ? first[..at] : first).ToLowerInvariant();
        }
    }
}
