using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using MediatR;
using Application.Interfaces;
using Application.Telegram.Commands;
using Domain.Entities;

namespace Application.Telegram.Dispatcher
{
    public class TelegramUpdateDispatcher : IUpdateDispatcher
    {
        private readonly IMediator _mediator;
        private readonly IUserStateService _stateService;

        public TelegramUpdateDispatcher(IMediator mediator, IUserStateService stateService)
        {
            _mediator = mediator;
            _stateService = stateService;
        }

        public async Task HandleAsync(Update update)
        {
            var message = update.Message;
            if (message == null)
                return;

            var chatId = message.Chat.Id;

            if (message.Text?.Trim().ToLower() == "/start")
            {
                await _mediator.Send(new CreateUserCommand
                {
                    TelegramUserId = chatId,
                    FirstName = message.From?.FirstName,
                    LastName = message.From?.LastName,
                    Username = message.From?.Username
                });

                await _mediator.Send(new StartCommand
                {
                    ChatId = chatId,
                    FirstName = message.From?.FirstName
                });
                return;
            }

            if (message.Type == MessageType.Document)
            {
                var step = await _stateService.GetStepAsync(chatId);
                await _mediator.Send(new UploadDocumentCommand
                {
                    TelegramUserId = chatId,
                    FileId = message.Document!.FileId,
                    FileName = message.Document.FileName,
                    FileType = step == "awaiting_passport" ? "passport" : "car_registration"
                });
                return;
            }

            await _mediator.Send(new UnknownCommand { ChatId = chatId });
        }
    }
}
