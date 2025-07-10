using Application.Telegram.Commands;
using Infrastructure.Telegram.Interface;
using MediatR;

namespace Application.Telegram.Handlers
{
    public class SendTextCommandHandler : IRequestHandler<SendTextCommand, Unit>
    {
        private readonly ITelegramBotService _botService;

        public SendTextCommandHandler(ITelegramBotService botService)
        {
            _botService = botService;
        }

        public async Task<Unit> Handle(SendTextCommand request, CancellationToken cancellationToken)
        {
            await _botService.SendTextAsync(request.ChatId, request.Message);
            return Unit.Value;
        }
    }
}
