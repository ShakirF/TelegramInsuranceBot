using MediatR;

namespace Application.Telegram.Commands
{
    public class SendTextCommand : IRequest<Unit>
    {
        public long ChatId { get; set; }
        public string Message { get; set; } = null!;
    }
}
