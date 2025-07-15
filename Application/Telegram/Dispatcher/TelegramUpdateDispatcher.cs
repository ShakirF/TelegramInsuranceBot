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
        private readonly IPromptProvider _promptProvider;

        public TelegramUpdateDispatcher(IMediator mediator, IUserStateService stateService, IPromptProvider promptProvider)
        {
            _mediator = mediator;
            _stateService = stateService;
            _promptProvider = promptProvider;
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
                    FileName = message.Document.FileName!,
                    FileType = step == UserStep.AwaitingPassport ? "passport" : "car_registration"
                });
                return;
            }

            if (message.Type == MessageType.Photo && message.Photo?.Any() == true)
            {
                var photo = message.Photo.Last(); // highest quality
                var step = await _stateService.GetStepAsync(chatId);
                var filename = $"photo_{photo.FileUniqueId}.jpg";

                await _mediator.Send(new UploadDocumentCommand
                {
                    TelegramUserId = chatId,
                    FileId = photo.FileId,
                    FileName = filename,
                    FileType = step == UserStep.AwaitingPassport ? "passport" : "car_registration"
                });

                return;
            }

            if (message.Text?.Trim().ToLower() == "confirm")
            {
                await _mediator.Send(new ConfirmPolicyCommand { ChatId = chatId });
                return;
            }

            if (message.Text?.Trim().ToLower() == "cancel")
            {
                var step = await _stateService.GetStepAsync(chatId);

                if (step == UserStep.AwaitingPriceConfirmation)
                {
                    var retryCount = await _stateService.GetCancelRetryCountAsync(chatId);

                    if (retryCount == 0)
                    {
                        await _mediator.Send(new SendTextCommand { ChatId = chatId, Message = await _promptProvider.GetPolicyFixPriceMessageAsync() });
                        await _stateService.IncrementCancelRetryCountAsync(chatId);
                        return;
                    }

                    await _mediator.Send(new CancelPolicyCommand { ChatId = chatId });
                    await _stateService.ResetCancelRetryCountAsync(chatId);
                    return;
                }

                await _mediator.Send(new CancelPolicyCommand { ChatId = chatId });
                return;
            }

            if (message.Text?.Trim().ToLower() == "/resendpolicy")
            {
                await _mediator.Send(new ResendPolicyCommand { ChatId = chatId });
                return;
            }

            if (message.Text?.Trim().ToLower() == "/simulateocr")
            {
                await _mediator.Send(new SimulateOcrCommand { ChatId = chatId });
                return;
            }
            if (message.Text?.Trim().ToLower() == "/adminsummary")
            {
                await _mediator.Send(new AdminSummaryCommand { ChatId = chatId });
                return;
            }
            if (message.Text?.Trim().ToLower() == "/logs")
            {
                await _mediator.Send(new LogsCommand { ChatId = chatId });
                return;
            }

            await _mediator.Send(new UnknownCommand { ChatId = chatId });
        }
    }
}
