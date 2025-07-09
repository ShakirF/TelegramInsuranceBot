namespace Application.Interfaces
{
    public interface IUserStateService
    {
        Task SetStepAsync(long telegramUserId, string step);
    }
}
