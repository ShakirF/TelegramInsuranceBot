using Application.Telegram.Commands;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.DbContext;

namespace Application.Telegram.Handlers
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateUserCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.Query().FirstOrDefaultAsync(x => x.TelegramUserId == request.TelegramUserId, cancellationToken);
            if (user != null)
                return;

            user = new User
            {
                TelegramUserId = request.TelegramUserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Username = request.Username,
            };

            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.UserStates.AddAsync(new UserState
            {
                TelegramUserId = request.TelegramUserId,
                CurrentStep = UserStep.Start,
                UpdatedAt = DateTime.UtcNow,
                UserId = user.Id
            }, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
