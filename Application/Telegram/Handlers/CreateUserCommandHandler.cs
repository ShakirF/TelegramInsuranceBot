using Application.Telegram.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.DbContext;

namespace Application.Telegram.Handlers
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
    {
        private readonly AppDbContext _context;

        public CreateUserCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.TelegramUserId == request.TelegramUserId, cancellationToken);
            if (user != null)
                return;

            user = new User
            {
                TelegramUserId = request.TelegramUserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Username = request.Username,
            };

            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _context.UserStates.AddAsync(new UserState
            {
                TelegramUserId = request.TelegramUserId,
                CurrentStep = "start",
                UpdatedAt = DateTime.UtcNow,
                UserId = user.Id
            });

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
