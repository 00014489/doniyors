using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace telegram_bot.Handlers
{
    public class UpdateHandler
    {
        private readonly MessageHandler _messageHandler;
        private readonly CallbackQueryHandler _callbackQueryHandler;
        public UpdateHandler(MessageHandler messageHandler, CallbackQueryHandler callbackQueryHandler)
        {
            _messageHandler = messageHandler;
            _callbackQueryHandler = callbackQueryHandler;
        }
        public async Task HandleAsync(Update update)
        {
            if (update.Message != null)
            {
                await _messageHandler.HandleAsync(update.Message);
            }
            else if (update.CallbackQuery != null)
            {
                await _callbackQueryHandler.HandleAsync(update.CallbackQuery);
            }
        }
    }
}