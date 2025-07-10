using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.DbContext;

namespace Application.Services
{
    public class UserStateService : IUserStateService
    {
        private readonly AppDbContext _context;

        public UserStateService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserStep> GetStepAsync(long telegramUserId)
        {
            var state = await _context.UserStates
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TelegramUserId == telegramUserId);

            return state?.CurrentStep ?? UserStep.Start;
        }
        public async Task SetStepAsync(long telegramUserId, UserStep step)
        {
            var state = await _context.UserStates
                .FirstOrDefaultAsync(s => s.TelegramUserId == telegramUserId);

            if (state == null)
            {
                state = new Domain.Entities.UserState
                {
                    TelegramUserId = telegramUserId,
                    CurrentStep = step
                };
                _context.UserStates.Add(state);
            }
            else
            {
                state.CurrentStep = step;
                state.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
