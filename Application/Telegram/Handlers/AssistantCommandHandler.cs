using Application.Interfaces;
using Application.Telegram.Commands;
using Infrastructure.Telegram.Interface;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Telegram.Handlers
{
    public class AssistantCommandHandler : IRequestHandler<AssistantCommand, Unit>
    {
        private readonly ITelegramBotService _bot;
        private readonly IPromptProvider _promptProvider;
        private readonly ILogger<AssistantCommandHandler> _logger;

        public AssistantCommandHandler(
            ITelegramBotService bot,
            IPromptProvider promptProvider,
            ILogger<AssistantCommandHandler> logger)
        {
            _bot = bot;
            _promptProvider = promptProvider;
            _logger = logger;
        }

        public async Task<Unit> Handle(AssistantCommand request, CancellationToken cancellationToken)
        {
            var reply = await _promptProvider.GetAssistentAnswer(request.UserMessage, cancellationToken);

            if (string.IsNullOrWhiteSpace(reply))
            {
                _logger.LogWarning("No answer generated for user {ChatId}", request.ChatId);
                return Unit.Value;
            }

            await _bot.SendTextAsync(request.ChatId, reply);

            _logger.LogInformation("Assistant replied to user {ChatId}", request.ChatId);

            return Unit.Value;
        }
    }
}
