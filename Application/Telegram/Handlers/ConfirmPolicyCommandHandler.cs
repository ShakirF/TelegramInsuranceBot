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
        private readonly ITelegramBotService _bot;
        private readonly ILogger<ConfirmPolicyCommandHandler> _logger;

        public ConfirmPolicyCommandHandler(IUserStateService stateService, ITelegramBotService bot, ILogger<ConfirmPolicyCommandHandler> logger)
        {
            _stateService = stateService;
            _bot = bot;
            _logger = logger;
        }

        public async Task<Unit> Handle(ConfirmPolicyCommand request, CancellationToken cancellationToken)
        {
            var step = await _stateService.GetStepAsync(request.ChatId);

            if (step != UserStep.AwaitingConfirmation)
            {
                await _bot.SendTextAsync(request.ChatId, "⚠️ You are not in the confirmation stage. Please upload your documents first.");
                return Unit.Value;
            }

            await _stateService.SetStepAsync(request.ChatId, UserStep.Confirmed);

            await _bot.SendTextAsync(request.ChatId, "✅ Thank you. I will now generate your policy document.");

            _logger.LogInformation("User {ChatId} confirmed extracted data and moved to 'confirmed' state.", request.ChatId);

            return Unit.Value;
        }
    }
}
