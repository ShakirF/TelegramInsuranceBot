using Domain.Enums;

namespace Application.Interfaces
{
    public interface IUserStateService
    {
        Task<UserStep> GetStepAsync(long telegramUserId);
        Task SetStepAsync(long telegramUserId, UserStep step);
    }
}
