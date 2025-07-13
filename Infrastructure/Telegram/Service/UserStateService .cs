using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class UserStateService : IUserStateService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserStateService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<UserStep> GetStepAsync(long telegramUserId)
        {
            var state = await _unitOfWork.UserStates.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TelegramUserId == telegramUserId);

            return state?.CurrentStep ?? UserStep.Start;
        }

        public async Task SetStepAsync(long telegramUserId, UserStep step)
        {
            var state = await _unitOfWork.UserStates.Query()
                .FirstOrDefaultAsync(s => s.TelegramUserId == telegramUserId);

            if (state == null)
            {
                state = new UserState { TelegramUserId = telegramUserId, CurrentStep = step };
                await _unitOfWork.UserStates.AddAsync(state);
            }
            else
            {
                state.CurrentStep = step;
                state.UpdatedAt = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> GetCancelRetryCountAsync(long telegramUserId)
        {
            var state = await _unitOfWork.UserStates.Query()
                .FirstOrDefaultAsync(s => s.TelegramUserId == telegramUserId);

            return state?.CancelRetryCount ?? 0;
        }

        public async Task IncrementCancelRetryCountAsync(long telegramUserId)
        {
            var state = await _unitOfWork.UserStates.Query()
                .FirstOrDefaultAsync(s => s.TelegramUserId == telegramUserId);
            if (state != null)
            {
                state.CancelRetryCount += 1;
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task ResetCancelRetryCountAsync(long telegramUserId)
        {
            var state = await _unitOfWork.UserStates.Query()
                .FirstOrDefaultAsync(s => s.TelegramUserId == telegramUserId);
            if (state != null)
            {
                state.CancelRetryCount = 0;
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
