namespace Application.Interfaces
{
    public interface IUserStateService
    {
        Task<string> GetStepAsync(long telegramUserId);
        Task SetStepAsync(long telegramUserId, string step);
    }
}
