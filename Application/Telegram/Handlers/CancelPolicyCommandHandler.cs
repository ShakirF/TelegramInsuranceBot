using Application.Interfaces;
using Application.Telegram.Commands;
using Domain.Enums;
using Infrastructure.Telegram.Interface;
using MediatR;

namespace Application.Telegram.Handlers
{
    public class CancelPolicyCommandHandler : IRequestHandler<CancelPolicyCommand, Unit>
    {
        private readonly IUserStateService _stateService;
        private readonly ITelegramBotService _bot;
        private readonly IMessageProvider _messageProvider;

        public CancelPolicyCommandHandler(IUserStateService stateService, ITelegramBotService bot, IMessageProvider messageProvider)
        {
            _stateService = stateService;
            _bot = bot;
            _messageProvider = messageProvider;
        }

        public async Task<Unit> Handle(CancelPolicyCommand request, CancellationToken cancellationToken)
        {
            await _stateService.SetStepAsync(request.ChatId, UserStep.Start);
            await _bot.SendTextAsync(request.ChatId, _messageProvider.GetPolicyCancelMessage());
            return Unit.Value;
        }
    }
}
