using MediatR;

namespace Application.Telegram.Commands
{
    public class GeneratePolicyCommand : IRequest<Unit>
    {
        public long ChatId { get; set; }
    }
}
