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
        private readonly IPromptProvider _promptProvider;

        public CancelPolicyCommandHandler(IUserStateService stateService, ITelegramBotService bot, IPromptProvider promptProvider)
        {
            _stateService = stateService;
            _bot = bot;
            _promptProvider = promptProvider;
        }

        public async Task<Unit> Handle(CancelPolicyCommand request, CancellationToken cancellationToken)
        {
            await _stateService.SetStepAsync(request.ChatId, UserStep.Start);
            await _bot.SendTextAsync(request.ChatId, await _promptProvider.GetOcrDoneMessageAsync());
            return Unit.Value;
        }
    }
}
