using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace telegram_bot.Models
{
    public class BotSettings
    {
        public string TELEGRAM_BOT_TOKEN { get; set; } = default!;
        public string WEBHOOK_BASE_URL { get; set; } = default!;
    }
}