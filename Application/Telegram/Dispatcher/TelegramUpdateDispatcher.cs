using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using MediatR;
using Application.Interfaces;
using Application.Telegram.Commands;
using Domain.Entities;
using Domain.Enums;

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
                    FileType = step == UserStep.AwaitingPassport ? "passport" : "car_registration"
                });
                return;
            }
            if (message.Text?.Trim().ToLower() == "confirm")
            {
                var step = await _stateService.GetStepAsync(chatId);
                if (step == UserStep.AwaitingConfirmation)
                {
                    await _mediator.Send(new ConfirmPolicyCommand
                    {
                        ChatId = chatId
                    });

                    return;
                }
                var messageText = step switch
                {
                    UserStep.AwaitingPassport => "⚠️ You're not in the confirmation step. Please upload your passport first.",
                    UserStep.AwaitingCarDoc => "⚠️ You're not in the confirmation step. Please upload your car registration document first.",
                    _ => "⚠️ You're not in the confirmation step. Please upload your documents first."
                };
                await _mediator.Send(new SendTextCommand {ChatId= chatId, Message = messageText });
                return;
            }

            await _mediator.Send(new UnknownCommand { ChatId = chatId });
        }
    }
}
