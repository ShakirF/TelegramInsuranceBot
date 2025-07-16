using MediatR;

namespace Application.Telegram.Commands
{
    public class AssistantCommand : IRequest<Unit>
    {
        public long ChatId { get; set; }
        public string UserMessage { get; set; } = default!;
    }
}
