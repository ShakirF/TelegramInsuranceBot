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

        public StartCommandHandler(ITelegramBotService botService, IUserStateService stateService)
        {
            _botService = botService;
            _stateService = stateService;
        }

        public async Task<Unit> Handle(StartCommand request, CancellationToken cancellationToken)
        {
            string introText = $"Hello {request.FirstName}, I will help you get insured!\n" +
                                        "📤 Please upload your passport document to begin.";
            await _botService.SendTextAsync(request.ChatId, introText);

            await _stateService.SetStepAsync(request.ChatId, UserStep.AwaitingPassport);

            return Unit.Value;
        }

    }
}
