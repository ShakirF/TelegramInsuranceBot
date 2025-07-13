using Application.Interfaces;
using Application.Telegram.Commands;
using Infrastructure.Telegram.Interface;
using MediatR;

namespace Application.Telegram.Handlers
{
    public class UnknownCommandHandler : IRequestHandler<UnknownCommand, Unit>
    {
        private readonly ITelegramBotService _botService;
        private readonly IPromptProvider _promptProvider;

        public UnknownCommandHandler(ITelegramBotService botService, IPromptProvider promptProvider)
        {
            _botService = botService;
            _promptProvider = promptProvider;
        }

        public async Task<Unit> Handle(UnknownCommand request, CancellationToken cancellationToken)
        {
            var message = await _promptProvider.GetUnknownCommandMessageAsync();
            await _botService.SendTextAsync(request.ChatId, message);
            return Unit.Value;
        }
    }
}
