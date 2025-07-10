using MediatR;

namespace Application.Telegram.Commands
{
    public class UnknownCommand : IRequest<Unit>
    {
        public long ChatId { get; set; }
    }
}
