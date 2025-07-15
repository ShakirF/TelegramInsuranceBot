using MediatR;

namespace Application.Telegram.Commands
{
    public class AdminSummaryCommand : IRequest<Unit>
    {
        public long ChatId { get; set; }
    }
}
