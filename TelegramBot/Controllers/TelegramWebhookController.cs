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
        private readonly ITelegramBotService _botService;
        private readonly IMediator _mediator;

        public TelegramWebhookController(ITelegramBotService botService, IMediator mediator)
        {
            _botService = botService;
            _mediator = mediator;
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
                var createUser = new CreateUserCommand
                {
                    TelegramUserId = message.Chat.Id,
                    FirstName = message.From?.FirstName,
                    LastName = message.From?.LastName,
                    Username = message.From?.Username,
                };
                await _mediator.Send(createUser);

                var startCommand = new StartCommand
                {
                    ChatId = message.Chat.Id,
                    FirstName = message.From?.FirstName
                };
                await _mediator.Send(startCommand);
            }
            else
            {
                await _botService.SendTextAsync(message.Chat.Id, "Unknown command. Please type /start to begin.");
            }

            return Ok();
        }
    }
}
