using Telegram.Bot.Types;

namespace Application.Interfaces
{
    public interface IUpdateDispatcher
    {
        Task HandleAsync(Update update);
    }
}
