using Application.Interfaces;
using Application.Telegram.Commands;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Telegram.Interface;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Constants;

namespace Application.Telegram.Handlers
{
    public class ConfirmPolicyCommandHandler : IRequestHandler<ConfirmPolicyCommand, Unit>
    {
        private readonly IUserStateService _stateService;
        private readonly ITelegramBotService _botService;
        private readonly ILogger<ConfirmPolicyCommandHandler> _logger;
        private readonly IPromptProvider _promptProvider;
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;

        public ConfirmPolicyCommandHandler(
            IUserStateService stateService,
            ITelegramBotService botService,
            ILogger<ConfirmPolicyCommandHandler> logger,
            IPromptProvider promptProvider,
            IMediator mediator,
            IUnitOfWork unitOfWork)
        {
            _stateService = stateService;
            _botService = botService;
            _logger = logger;
            _promptProvider = promptProvider;
            _mediator = mediator;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(ConfirmPolicyCommand request, CancellationToken cancellationToken)
        {
            var step = await _stateService.GetStepAsync(request.ChatId);

            if (step == UserStep.AwaitingConfirmation)
            {
                await _stateService.SetStepAsync(request.ChatId, UserStep.AwaitingPriceConfirmation);

                await _botService.SendTextAsync(request.ChatId, await _promptProvider.GetPriceQuoteMessageAsync($"{PolicyConstants.DefaultPolicyPrice} USD"));

                _logger.LogInformation("User {ChatId} confirmed data, moved to AwaitingPriceConfirmation.", request.ChatId);
                return Unit.Value;
            }

            if (step == UserStep.AwaitingPriceConfirmation)
            {
                await _stateService.SetStepAsync(request.ChatId, UserStep.Confirmed);

                await _botService.SendTextAsync(request.ChatId, await _promptProvider.GetPolicyConfirmedMessageAsync());

                _logger.LogInformation("User {ChatId} confirmed final price. Status: Confirmed.", request.ChatId);

                await _unitOfWork.PolicyEvents.AddAsync(new PolicyEvent
                {
                    PolicyId = 0, 
                    EventType = EventType.Pending.ToString()
                }, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Simulate delay
                _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));

                    try
                    {
                        await _mediator.Send(new GeneratePolicyCommand
                        {
                            ChatId = request.ChatId
                        });
                    }
                    catch (Exception ex)
                    {
                        await _unitOfWork.PolicyEvents.AddAsync(new PolicyEvent
                        {
                            PolicyId = 0,
                            EventType = EventType.Failed.ToString(),
                        });
                        await _unitOfWork.Errors.AddAsync(new Error
                        {
                            TelegramUserId = request.ChatId,
                            Message = ex.Message,
                            StackTrace = ex.ToString()
                        });
                        await _unitOfWork.SaveChangesAsync();
                    }
                });

                await _mediator.Send(new GeneratePolicyCommand { ChatId = request.ChatId }, cancellationToken);
                return Unit.Value;
            }

            await _botService.SendTextAsync(request.ChatId,await _promptProvider.GetStepMismatchMessageAsync(step));

            return Unit.Value;
        }

    }
}
