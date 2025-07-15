using MediatR;

namespace Application.Telegram.Commands
{
    public class SimulateOcrCommand : IRequest<Unit>
    {
        public long ChatId { get; set; }
    }
}
