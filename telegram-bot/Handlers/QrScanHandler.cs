using System.Globalization;
using Doniyors.Data.Entities;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using telegram_bot.Keyboards;
using telegram_bot.Models;
using telegram_bot.Services;
using telegram_bot.Services.Localization;

namespace telegram_bot.Handlers
{
    /// <summary>
    /// The administrator's "Scan QR" flow:
    /// <list type="number">
    /// <item>"Scan QR" lists the adventures that can be scanned for.</item>
    /// <item>Picking one moves the administrator to <see cref="SessionStep.WaitingForPhoto"/>.</item>
    /// <item>Every photo sent after that is searched for members' QR codes, and
    /// each member found is credited with the adventure's points.</item>
    /// <item>Cancel ends the session.</item>
    /// </list>
    /// </summary>
    public class QrScanHandler
    {
        /// <summary>The Bot API refuses to hand a bot any file larger than this.</summary>
        private const long MaxDownloadBytes = 20 * 1024 * 1024;

        private readonly ITelegramBotClient _bot;
        private readonly UserService _userService;
        private readonly SessionService _sessionService;
        private readonly QrScanService _scanService;
        private readonly ReplyKeyboards _replyKeyboards;
        private readonly InlineKeyboards _inlineKeyboards;
        private readonly ILocalizationService _localization;
        private readonly ILogger<QrScanHandler> _logger;

        public QrScanHandler(
            ITelegramBotClient bot,
            UserService userService,
            SessionService sessionService,
            QrScanService scanService,
            ReplyKeyboards replyKeyboards,
            InlineKeyboards inlineKeyboards,
            ILocalizationService localization,
            ILogger<QrScanHandler> logger)
        {
            _bot = bot;
            _userService = userService;
            _sessionService = sessionService;
            _scanService = scanService;
            _replyKeyboards = replyKeyboards;
            _inlineKeyboards = inlineKeyboards;
            _localization = localization;
            _logger = logger;
        }

        /// <summary>The "Scan QR" button: offers the adventures to scan for.</summary>
        public async Task StartAsync(
            Message message,
            CancellationToken cancellationToken = default)
        {
            var admin = await _userService.GetUserByTgIdAsync(message.From!.Id, cancellationToken);
            var lang = admin?.LanguageCode ?? string.Empty;

            if (!QrScanService.CanScan(admin))
            {
                await _bot.SendMessage(
                    message.Chat.Id,
                    _localization.Get(lang, "scan_not_admin"),
                    cancellationToken: cancellationToken);

                return;
            }

            var adventures = await _scanService.GetScannableAdventuresAsync(cancellationToken);

            if (adventures.Count == 0)
            {
                await _bot.SendMessage(
                    message.Chat.Id,
                    _localization.Get(lang, "scan_no_adventures"),
                    cancellationToken: cancellationToken);

                return;
            }

            await _bot.SendMessage(
                message.Chat.Id,
                _localization.Get(lang, "scan_choose_adventure"),
                replyMarkup: _inlineKeyboards.ScanAdventures(adventures),
                cancellationToken: cancellationToken);
        }

        /// <summary>A tap on one of the adventures offered by <see cref="StartAsync"/>.</summary>
        public async Task SelectAdventureAsync(
            CallbackQuery callback,
            int travelId,
            CancellationToken cancellationToken = default)
        {
            // Answered first, so the button stops spinning whatever happens next.
            await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: cancellationToken);

            var chatId = callback.Message?.Chat.Id ?? callback.From.Id;

            var admin = await _userService.GetUserByTgIdAsync(callback.From.Id, cancellationToken);
            var lang = admin?.LanguageCode ?? string.Empty;

            if (!QrScanService.CanScan(admin))
            {
                await _bot.SendMessage(
                    chatId,
                    _localization.Get(lang, "scan_not_admin"),
                    cancellationToken: cancellationToken);

                return;
            }

            // The picker may be old: the adventure can have been disabled since.
            var adventure = await _scanService.GetScannableAdventureAsync(travelId, cancellationToken);

            if (adventure is null)
            {
                await _bot.SendMessage(
                    chatId,
                    _localization.Get(lang, "scan_adventure_unavailable"),
                    cancellationToken: cancellationToken);

                return;
            }

            await _sessionService.StartQrScanAsync(callback.From.Id, adventure.Id, cancellationToken);

            if (callback.Message is { } picker)
                await RemovePickerButtonsAsync(picker, cancellationToken);

            await _bot.SendMessage(
                chatId,
                Format(lang, "scan_send_photos", adventure.Title, adventure.Points),
                replyMarkup: _replyKeyboards.Cancel(lang),
                cancellationToken: cancellationToken);
        }

        /// <summary>Any message from an administrator in the scanning step.</summary>
        public async Task HandleSessionMessageAsync(
            Message message,
            CancellationToken cancellationToken = default)
        {
            var tgUserId = message.From!.Id;

            var admin = await _userService.GetUserByTgIdAsync(tgUserId, cancellationToken);
            var lang = admin?.LanguageCode ?? string.Empty;

            // Cancel comes before every other check, so nobody can get stuck in
            // the session — not even an administrator who lost the role mid-scan.
            if (message.Text is { } text && _localization.IsTranslation("cancel", text))
            {
                await FinishAsync(message.Chat.Id, tgUserId, lang, "cancel_message", cancellationToken);
                return;
            }

            if (!QrScanService.CanScan(admin))
            {
                await FinishAsync(message.Chat.Id, tgUserId, lang, "scan_not_admin", cancellationToken);
                return;
            }

            var travelId = await _sessionService.GetQrScanAdventureIdAsync(tgUserId, cancellationToken);

            var adventure = travelId is int id
                ? await _scanService.GetActiveAdventureAsync(id, cancellationToken)
                : null;

            if (adventure is null)
            {
                await FinishAsync(message.Chat.Id, tgUserId, lang, "scan_adventure_unavailable", cancellationToken);
                return;
            }

            var image = GetImage(message);

            if (image is null)
            {
                await _bot.SendMessage(
                    message.Chat.Id,
                    _localization.Get(lang, "scan_send_photo_hint"),
                    replyMarkup: _replyKeyboards.Cancel(lang),
                    cancellationToken: cancellationToken);

                return;
            }

            if (image.FileSize > MaxDownloadBytes)
            {
                await ReplyAsync(message, _localization.Get(lang, "scan_file_too_large"), cancellationToken);
                return;
            }

            using var buffer = new MemoryStream();

            await _bot.GetInfoAndDownloadFile(image.FileId, buffer, cancellationToken);

            buffer.Position = 0;

            var codes = _scanService.ReadCodes(buffer);

            if (codes.Count == 0)
            {
                await ReplyAsync(message, _localization.Get(lang, "scan_no_qr_found"), cancellationToken);
                return;
            }

            var awards = await _scanService.AwardAsync(codes, adventure, tgUserId, cancellationToken);

            await ReplyAsync(
                message,
                string.Join('\n', awards.Select(award => Describe(lang, award, adventure))),
                cancellationToken);
        }

        /// <summary>
        /// The picture in a message: the largest size of a photo, or a document
        /// that is an image. Documents matter — a photo sent as a file skips
        /// Telegram's recompression, which is what rescues a small or distant code.
        /// </summary>
        private static FileBase? GetImage(Message message)
        {
            if (message.Photo is { Length: > 0 } sizes)
                return sizes[^1];

            if (message.Document is { MimeType: { } mimeType } document
                && mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return document;
            }

            return null;
        }

        private string Describe(string lang, ScanAward award, ScanAdventure adventure)
        {
            var member = MemberName(award);

            return award.Status switch
            {
                ScanAwardStatus.Awarded =>
                    Format(lang, "scan_awarded", member, adventure.Points, award.Balance),

                ScanAwardStatus.AlreadyAwarded =>
                    Format(lang, "scan_already_awarded", member),

                ScanAwardStatus.UserDisabled =>
                    Format(lang, "scan_user_disabled", member),

                _ => _localization.Get(lang, "scan_unknown_code"),
            };
        }

        /// <summary>
        /// Name and @username together where both exist, so the administrator
        /// can check the credit went to the person standing in front of them.
        /// </summary>
        private static string MemberName(ScanAward award)
        {
            var hasFirstName = !string.IsNullOrWhiteSpace(award.FirstName);
            var hasUserName = !string.IsNullOrWhiteSpace(award.UserName);

            return (hasFirstName, hasUserName) switch
            {
                (true, true) => $"{award.FirstName} (@{award.UserName})",
                (true, false) => award.FirstName,
                (false, true) => $"@{award.UserName}",
                _ => $"#{award.UserId}",
            };
        }

        /// <summary>Leaves the scanning step and puts the main menu back in place of Cancel.</summary>
        private async Task FinishAsync(
            long chatId,
            long tgUserId,
            string lang,
            string messageKey,
            CancellationToken cancellationToken)
        {
            await _sessionService.ClearAsync(tgUserId, cancellationToken);

            await _bot.SendMessage(
                chatId,
                _localization.Get(lang, messageKey),
                replyMarkup: await _replyKeyboards.MainMenu(tgUserId, lang, cancellationToken),
                cancellationToken: cancellationToken);
        }

        /// <summary>Quotes the photo, so results stay matched to it when an album arrives.</summary>
        private Task ReplyAsync(Message message, string text, CancellationToken cancellationToken)
        {
            return _bot.SendMessage(
                message.Chat.Id,
                text,
                replyParameters: message.MessageId,
                cancellationToken: cancellationToken);
        }

        /// <summary>Stops the picker being tapped again once a choice is made.</summary>
        private async Task RemovePickerButtonsAsync(Message picker, CancellationToken cancellationToken)
        {
            try
            {
                await _bot.EditMessageReplyMarkup(
                    picker.Chat.Id,
                    picker.MessageId,
                    replyMarkup: null,
                    cancellationToken: cancellationToken);
            }
            catch (ApiRequestException ex)
            {
                // Cosmetic only — e.g. the message is too old to edit. The
                // session has already started.
                _logger.LogDebug(ex, "Could not remove the adventure picker's buttons.");
            }
        }

        private string Format(string lang, string key, params object[] args) =>
            string.Format(CultureInfo.InvariantCulture, _localization.Get(lang, key), args);
    }
}
