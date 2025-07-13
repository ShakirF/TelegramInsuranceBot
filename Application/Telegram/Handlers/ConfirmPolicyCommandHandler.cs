using Application.Interfaces;
using Application.Telegram.Commands;
using Domain.Enums;
using Infrastructure.Telegram.Interface;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Telegram.Handlers
{
    public class ConfirmPolicyCommandHandler : IRequestHandler<ConfirmPolicyCommand, Unit>
    {
        private readonly IUserStateService _stateService;
        private readonly ITelegramBotService _botService;
        private readonly ILogger<ConfirmPolicyCommandHandler> _logger;
        private readonly IMessageProvider _messageProvider;

        public ConfirmPolicyCommandHandler(
            IUserStateService stateService,
            ITelegramBotService botService,
            ILogger<ConfirmPolicyCommandHandler> logger,
            IMessageProvider messageProvider)
        {
            _stateService = stateService;
            _botService = botService;
            _logger = logger;
            _messageProvider = messageProvider;
        }

        public async Task<Unit> Handle(ConfirmPolicyCommand request, CancellationToken cancellationToken)
        {
            var step = await _stateService.GetStepAsync(request.ChatId);

            if (step == UserStep.AwaitingConfirmation)
            {
                await _stateService.SetStepAsync(request.ChatId, UserStep.AwaitingPriceConfirmation);

                await _botService.SendTextAsync(request.ChatId, _messageProvider.GetPriceQuoteMessage("100 USD"));

                _logger.LogInformation("User {ChatId} confirmed data, moved to AwaitingPriceConfirmation.", request.ChatId);
                return Unit.Value;
            }

            if (step == UserStep.AwaitingPriceConfirmation)
            {
                await _stateService.SetStepAsync(request.ChatId, UserStep.Confirmed);

                await _botService.SendTextAsync(request.ChatId, _messageProvider.GetPolicyConfirmedMessage());

                _logger.LogInformation("User {ChatId} confirmed final price. Status: Confirmed.", request.ChatId);
                return Unit.Value;
            }

            await _botService.SendTextAsync(request.ChatId, _messageProvider.GetStepMismatchMessage(step));

            return Unit.Value;
        }

    }
}
