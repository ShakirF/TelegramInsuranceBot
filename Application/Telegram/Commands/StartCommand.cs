using MediatR;

namespace Application.Telegram.Commands
{
    public class StartCommand : IRequest<Unit>
    {
        public long ChatId { get; set; }
        public string? FirstName { get; set; }
    }

}
