using MediatR;

namespace Application.Telegram.Commands
{
    public class CancelPolicyCommand : IRequest<Unit>
    {
        public long ChatId { get; set; }
    }
}
