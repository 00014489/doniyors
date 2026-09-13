using Telegram.Bot;
using Telegram.Bot.Types;
using Doniyors.Data.Entities;
using telegram_bot.Keyboards;
using telegram_bot.Services;
using telegram_bot.Services.Localization;

namespace telegram_bot.Handlers
{
    public class CallbackQueryHandler
    {
        private readonly ITelegramBotClient _bot;
        private readonly ReplyKeyboards _replyKeyboards;
        private readonly UserService _userService;
        private readonly SessionService _sessionService;
        private readonly ILocalizationService _localization;
        private readonly QrScanHandler _qrScanHandler;

        public CallbackQueryHandler(
            ITelegramBotClient bot,
            ReplyKeyboards replyKeyboards,
            UserService userService,
            SessionService sessionService,
            ILocalizationService localization,
            QrScanHandler qrScanHandler)
        {
            _bot = bot;
            _replyKeyboards = replyKeyboards;
            _userService = userService;
            _sessionService = sessionService;
            _localization = localization;
            _qrScanHandler = qrScanHandler;
        }

        public async Task HandleAsync(
            CallbackQuery callback,
            CancellationToken cancellationToken = default)
        {
            // Carries an id, so it cannot be a case label below.
            if (InlineKeyboards.TryParseScanAdventure(callback.Data, out var travelId))
            {
                await _qrScanHandler.SelectAdventureAsync(callback, travelId, cancellationToken);
                return;
            }

            // Answered first, so the button stops spinning whatever happens next.
            await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: cancellationToken);

            switch (callback.Data)
            {
                case "change_language":
                    await StartLanguageChoiceAsync(callback, cancellationToken);
                    break;
            }
        }

        /// <summary>The inline counterpart of the "Change language" reply button.</summary>
        private async Task StartLanguageChoiceAsync(CallbackQuery callback, CancellationToken cancellationToken)
        {
            var user = await _userService.GetUserByTgIdAsync(callback.From.Id, cancellationToken);

            if (user is null)
                return;

            var chatId = callback.Message?.Chat.Id ?? callback.From.Id;

            await _sessionService.SetSessionStepAsync(user.TgUserId, SessionStep.WaitingForLanguage, cancellationToken);

            await _bot.SendMessage(
                chatId,
                _localization.Prompt(user.LanguageCode, "choose_language"),
                replyMarkup: _replyKeyboards.SelectLanguage(_localization.GetLanguageFullNames(), user.LanguageCode),
                cancellationToken: cancellationToken);
        }
    }
}
