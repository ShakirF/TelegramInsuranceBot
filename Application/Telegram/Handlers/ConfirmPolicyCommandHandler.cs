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
        private readonly IPromptProvider _promptProvider;

        public ConfirmPolicyCommandHandler(
            IUserStateService stateService,
            ITelegramBotService botService,
            ILogger<ConfirmPolicyCommandHandler> logger,
            IPromptProvider promptProvider)
        {
            _stateService = stateService;
            _botService = botService;
            _logger = logger;
            _promptProvider = promptProvider;
        }

        public async Task<Unit> Handle(ConfirmPolicyCommand request, CancellationToken cancellationToken)
        {
            var step = await _stateService.GetStepAsync(request.ChatId);

            if (step == UserStep.AwaitingConfirmation)
            {
                await _stateService.SetStepAsync(request.ChatId, UserStep.AwaitingPriceConfirmation);

                await _botService.SendTextAsync(request.ChatId, await _promptProvider.GetPriceQuoteMessageAsync("100 USD"));

                _logger.LogInformation("User {ChatId} confirmed data, moved to AwaitingPriceConfirmation.", request.ChatId);
                return Unit.Value;
            }

            if (step == UserStep.AwaitingPriceConfirmation)
            {
                await _stateService.SetStepAsync(request.ChatId, UserStep.Confirmed);

                await _botService.SendTextAsync(request.ChatId, await _promptProvider.GetPolicyConfirmedMessageAsync());

                _logger.LogInformation("User {ChatId} confirmed final price. Status: Confirmed.", request.ChatId);
                return Unit.Value;
            }

            await _botService.SendTextAsync(request.ChatId,await _promptProvider.GetStepMismatchMessageAsync(step));

            return Unit.Value;
        }

    }
}
