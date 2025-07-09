using Application.Interfaces;
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

        public async Task SetStepAsync(long telegramUserId, string step)
        {
            var state = await _context.UserStates
                .FirstOrDefaultAsync(s => s.TelegramUserId == telegramUserId);

            if (state != null)
            {
                state.CurrentStep = step;
                state.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
