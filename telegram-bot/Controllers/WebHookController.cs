using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using telegram_bot.Handlers;

namespace telegram_bot.Controllers
{
    [ApiController]
    [Route("webhook")]
    public class WebHookController: ControllerBase
    {
        private readonly UpdateHandler _handler;
        public WebHookController(UpdateHandler handler)
        {
            _handler = handler;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            Console.WriteLine($"Received update: {update.Id}");
            await _handler.HandleAsync(update);
            return Ok();
        }
    }
}