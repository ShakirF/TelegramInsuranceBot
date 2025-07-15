using MediatR;

namespace Application.Telegram.Commands
{
    public class ResendPolicyCommand : IRequest<Unit>
    {
        public long ChatId { get; set; }
    }
}
