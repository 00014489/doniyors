using Telegram.Bot.Types;

namespace telegram_bot.Handlers
{
    public class UpdateHandler
    {
        private readonly MessageHandler _messageHandler;
        private readonly CallbackQueryHandler _callbackQueryHandler;
        private readonly ILogger<UpdateHandler> _logger;

        public UpdateHandler(
            MessageHandler messageHandler,
            CallbackQueryHandler callbackQueryHandler,
            ILogger<UpdateHandler> logger)
        {
            _messageHandler = messageHandler;
            _callbackQueryHandler = callbackQueryHandler;
            _logger = logger;
        }

        public async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
        {
            switch (update)
            {
                case { Message: not null }:
                    await _messageHandler.HandleAsync(update.Message, cancellationToken);
                    break;

                case { CallbackQuery: not null }:
                    await _callbackQueryHandler.HandleAsync(update.CallbackQuery, cancellationToken);
                    break;

                default:
                    // Only Message and CallbackQuery are subscribed to, so this
                    // means the webhook's allowed-updates list drifted.
                    _logger.LogDebug("Ignored update {UpdateId} of type {Type}.", update.Id, update.Type);
                    break;
            }
        }
    }
}
