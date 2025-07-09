using Infrastructure.Telegram;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Handlers;

namespace TelegramBot.Controllers
{
    [ApiController]
    [Route("api/bot/update")]
    public class TelegramWebhookController : ControllerBase
    {
        private readonly TelegramBotService _botService;

        public TelegramWebhookController(TelegramBotService botService)
        {
            _botService = botService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            if (update == null || update.Type != UpdateType.Message)
                return Ok();

            var message = update.Message;
            if (message == null || message.Type != MessageType.Text || string.IsNullOrWhiteSpace(message.Text))
                return Ok();

            if (message.Text.Trim().ToLower() == "/start")
            {
                var handler = new StartCommandHandler(_botService);
                await handler.HandleAsync(message);
            }
            else
            {
                await _botService.SendTextAsync(message.Chat.Id, "Unknown command. Please type /start to begin.");
            }

            return Ok();
        }
    }
}
