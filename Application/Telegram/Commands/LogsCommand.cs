using MediatR;

namespace Application.Telegram.Commands
{
    public class LogsCommand : IRequest<Unit>
    {
        public long ChatId { get; set; }
    }
}
