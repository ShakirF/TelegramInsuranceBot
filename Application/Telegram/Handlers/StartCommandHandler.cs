using Application.Interfaces;
using Application.Telegram.Commands;
using Domain.Enums;
using Infrastructure.Telegram.Interface;
using MediatR;

namespace Application.Telegram.Handlers
{
    public class StartCommandHandler : IRequestHandler<StartCommand, Unit>
    {
        private readonly ITelegramBotService _botService;
        private readonly IUserStateService _stateService;
        private readonly IPromptProvider _promptProvider;

        public StartCommandHandler(ITelegramBotService botService, IUserStateService stateService, IPromptProvider promptProvider)
        {
            _botService = botService;
            _stateService = stateService;
            _promptProvider = promptProvider;
        }

        public async Task<Unit> Handle(StartCommand request, CancellationToken cancellationToken)
        {
           var message = await _promptProvider.GetStartMessageAsync(request.FirstName);
            await _botService.SendTextAsync(request.ChatId, message);

            await _stateService.SetStepAsync(request.ChatId, UserStep.AwaitingPassport);

            return Unit.Value;
        }

    }
}
