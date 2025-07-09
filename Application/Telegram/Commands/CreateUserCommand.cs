using MediatR;

namespace Application.Telegram.Commands
{
    public class CreateUserCommand : IRequest
    {
        public long TelegramUserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
    }
}
