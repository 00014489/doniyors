using System.Net;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Doniyors.Data.Entities;
using telegram_bot.Keyboards;
using telegram_bot.Services;
using telegram_bot.Services.Localization;
using DbUser = Doniyors.Data.Entities.User;

namespace telegram_bot.Handlers
{
    /// <summary>
    /// Every reply is written in the member's language as stored in
    /// <c>Users.LanguageCode</c>, read fresh for each message — the Mini App and
    /// the admin panel can change it at any moment. A member who has not chosen
    /// one yet gets the prompt in every language.
    /// </summary>
    public class MessageHandler
    {
        private readonly ITelegramBotClient _bot;
        private readonly CommandHandler _commandHandler;
        private readonly UserService _userService;
        private readonly SessionService _sessionService;
        private readonly ReplyKeyboards _replyKeyboards;
        private readonly ILocalizationService _localizationService;
        private readonly QrScanHandler _qrScanHandler;
        private readonly MiniAppMenuButton _menuButton;
        private readonly ILogger<MessageHandler> _logger;

        public MessageHandler(
            ITelegramBotClient bot,
            CommandHandler commandHandler,
            UserService userService,
            SessionService sessionService,
            ReplyKeyboards replyKeyboards,
            ILocalizationService localizationService,
            QrScanHandler qrScanHandler,
            MiniAppMenuButton menuButton,
            ILogger<MessageHandler> logger)
        {
            _bot = bot;
            _commandHandler = commandHandler;
            _userService = userService;
            _sessionService = sessionService;
            _replyKeyboards = replyKeyboards;
            _localizationService = localizationService;
            _qrScanHandler = qrScanHandler;
            _menuButton = menuButton;
            _logger = logger;
        }

        public async Task HandleAsync(
            Message message,
            CancellationToken cancellationToken = default)
        {
            // No sender (e.g. a channel post): nobody to answer, and no language
            // to answer in.
            if (message.From is null)
                return;

            // Read before looking at the message type: the QR scanning step
            // waits for photos, not text. A member with no session row reads
            // back as None, which is also "not in a flow".
            var sessionStep = await _sessionService.GetSessionStepAsync(message.From.Id, cancellationToken);

            switch (sessionStep)
            {
                case SessionStep.WaitingForPhoto:
                    await _qrScanHandler.HandleSessionMessageAsync(message, cancellationToken);
                    return;

                case SessionStep.WaitingForLanguage:
                    await HandleLanguageChoiceAsync(message, cancellationToken);
                    return;

                case not SessionStep.None:
                    await ReplyAsync(message, "in_session", withMainMenu: false, cancellationToken);
                    return;
            }

            if (message.Text is not { } text)
            {
                // Photos, documents, stickers…: nothing to do outside a flow.
                await ReplyAsync(message, "use_menu", withMainMenu: true, cancellationToken);
                return;
            }

            if (text.StartsWith('/'))
            {
                await _commandHandler.HandleAsync(message, cancellationToken);
            }
            else if (_localizationService.IsTranslation("scan_qr", text))
            {
                await _qrScanHandler.StartAsync(message, cancellationToken);
            }
            else if (_localizationService.IsTranslation("profile", text))
            {
                await ShowProfileAsync(message, cancellationToken);
            }
            else if (_localizationService.IsTranslation("main_menu", text))
            {
                await ReplyAsync(message, "returned_main_menu", withMainMenu: true, cancellationToken);
            }
            else if (_localizationService.IsTranslation("change_language", text))
            {
                await StartLanguageChoiceAsync(message, cancellationToken);
            }
            else
            {
                await ReplyAsync(message, "use_menu", withMainMenu: true, cancellationToken);
            }
        }

        private async Task ShowProfileAsync(Message message, CancellationToken cancellationToken)
        {
            var user = await FindUserAsync(message, cancellationToken);

            if (user is null)
                return;

            var lang = user.LanguageCode;

            var languageName = _localizationService.IsSupported(lang)
                ? _localizationService.Get(lang, "full_name")
                : "—";

            // HTML parse mode: a first name such as "<Ali>" would otherwise be
            // read as a tag and make Telegram reject the whole message.
            var firstName = WebUtility.HtmlEncode(message.From!.FirstName);

            var profileMessage =
                $"👤 <b>{firstName}</b>\n\n" +
                $"──────────────\n" +
                $"🌐 <b>{_localizationService.Get(lang, "language_is")}:</b> {languageName}\n" +
                $"📅 <b>{_localizationService.Get(lang, "registered_at")}:</b> {user.RegisteredAt:dd.MM.yyyy HH:mm}\n\n" +
                $"⭐ <b>{_localizationService.Get(lang, "points")}:</b> <tg-spoiler>{user.Points}</tg-spoiler>\n" +
                $"──────────────";

            await _bot.SendMessage(
                message.Chat.Id,
                profileMessage,
                parseMode: ParseMode.Html,
                replyMarkup: _replyKeyboards.ChangeLanguageButton(lang),
                cancellationToken: cancellationToken);
        }

        private async Task StartLanguageChoiceAsync(Message message, CancellationToken cancellationToken)
        {
            var user = await FindUserAsync(message, cancellationToken);

            if (user is null)
                return;

            await _sessionService.SetSessionStepAsync(user.TgUserId, SessionStep.WaitingForLanguage, cancellationToken);

            await _bot.SendMessage(
                message.Chat.Id,
                _localizationService.Prompt(user.LanguageCode, "choose_language"),
                replyMarkup: _replyKeyboards.SelectLanguage(_localizationService.GetLanguageFullNames(), user.LanguageCode),
                cancellationToken: cancellationToken);
        }

        private async Task HandleLanguageChoiceAsync(Message message, CancellationToken cancellationToken)
        {
            // A session row cannot outlive its user, so without one there is
            // nothing to leave — just point them at /start.
            var user = await FindUserAsync(message, cancellationToken);

            if (user is null)
                return;

            if (message.Text is { } text && _localizationService.IsTranslation("cancel", text))
            {
                await _sessionService.SetSessionStepAsync(user.TgUserId, SessionStep.None, cancellationToken);

                await _bot.SendMessage(
                    message.Chat.Id,
                    _localizationService.Prompt(user.LanguageCode, "cancel_message"),
                    replyMarkup: await _replyKeyboards.MainMenu(user.TgUserId, user.LanguageCode, cancellationToken),
                    cancellationToken: cancellationToken);

                return;
            }

            var langCode = message.Text is null
                ? null
                : _localizationService.GetLanguageCode(message.Text);

            if (langCode is null)
            {
                await _bot.SendMessage(
                    message.Chat.Id,
                    _localizationService.Prompt(user.LanguageCode, "another_lang"),
                    cancellationToken: cancellationToken);

                return;
            }

            await _userService.UpdateLanguage(user.TgUserId, langCode, cancellationToken);
            await _sessionService.SetSessionStepAsync(user.TgUserId, SessionStep.None, cancellationToken);
            await _menuButton.UpdateAsync(message.Chat.Id, langCode, cancellationToken);

            await _bot.SendMessage(
                message.Chat.Id,
                _localizationService.Get(langCode, "welcome"),
                replyMarkup: await _replyKeyboards.MainMenu(user.TgUserId, langCode, cancellationToken),
                cancellationToken: cancellationToken);
        }

        /// <summary>A text from language.json in the sender's language, optionally with the main menu.</summary>
        private async Task ReplyAsync(
            Message message,
            string key,
            bool withMainMenu,
            CancellationToken cancellationToken)
        {
            var user = await FindUserAsync(message, cancellationToken);

            if (user is null)
                return;

            var text = _localizationService.Prompt(user.LanguageCode, key);

            if (withMainMenu)
            {
                await _bot.SendMessage(
                    message.Chat.Id,
                    text,
                    replyMarkup: await _replyKeyboards.MainMenu(user.TgUserId, user.LanguageCode, cancellationToken),
                    cancellationToken: cancellationToken);
            }
            else
            {
                await _bot.SendMessage(message.Chat.Id, text, cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        /// The sender's row, or <c>null</c> after asking them to /start — without
        /// a row there is no stored language to answer in.
        /// </summary>
        private async Task<DbUser?> FindUserAsync(Message message, CancellationToken cancellationToken)
        {
            var user = await _userService.GetUserByTgIdAsync(message.From!.Id, cancellationToken);

            if (user is null)
            {
                _logger.LogInformation("Message from an unregistered user. TgUserId={TgUserId}", message.From.Id);

                await _bot.SendMessage(
                    message.Chat.Id,
                    _localizationService.Prompt(null, "start_first"),
                    cancellationToken: cancellationToken);
            }

            return user;
        }
    }
}
