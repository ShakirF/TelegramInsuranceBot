using MediatR;

namespace Application.Telegram.Commands
{
    public class ConfirmPolicyCommand : IRequest<Unit>
    {
        public long ChatId { get; set; }
    }
}
