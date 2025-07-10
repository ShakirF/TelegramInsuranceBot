using Application.Telegram.Commands;
using Infrastructure.Telegram.Interface;
using MediatR;

namespace Application.Telegram.Handlers
{
    public class UnknownCommandHandler : IRequestHandler<UnknownCommand, Unit>
    {
        private readonly ITelegramBotService _botService;

        public UnknownCommandHandler(ITelegramBotService botService)
        {
            _botService = botService;
        }

        public async Task<Unit> Handle(UnknownCommand request, CancellationToken cancellationToken)
        {
            await _botService.SendTextAsync(request.ChatId, "Unknown command or message. Please send a document or type /start.");
            return Unit.Value;
        }
    }
}
