using Application.Interfaces;
using Application.Telegram.Commands;
using Infrastructure.Telegram.Interface;
using MediatR;

namespace Application.Telegram.Handlers
{
    public class UnknownCommandHandler : IRequestHandler<UnknownCommand, Unit>
    {
        private readonly ITelegramBotService _botService;
        private readonly IMessageProvider _messageProvider;

        public UnknownCommandHandler(ITelegramBotService botService, IMessageProvider messageProvider)
        {
            _botService = botService;
            _messageProvider = messageProvider;
        }

        public async Task<Unit> Handle(UnknownCommand request, CancellationToken cancellationToken)
        {
            var message = _messageProvider.GetUnknownCommandMessage();
            await _botService.SendTextAsync(request.ChatId, message);
            return Unit.Value;
        }
    }
}
