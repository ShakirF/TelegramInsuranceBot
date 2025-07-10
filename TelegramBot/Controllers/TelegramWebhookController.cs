using Application.Interfaces;
using Application.Telegram.Commands;
using Infrastructure.Telegram.Interface;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.Controllers
{
    [ApiController]
    [Route("api/bot/update")]
    public class TelegramWebhookController : ControllerBase
    {
        private readonly IUpdateDispatcher _dispatcher;

        public TelegramWebhookController( IUpdateDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            await _dispatcher.HandleAsync(update);
            return Ok();
        }
    }
}
