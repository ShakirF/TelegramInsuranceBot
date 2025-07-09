using Infrastructure.Telegram;
using Telegram.Bot.Types;

namespace TelegramBot.Handlers
{
    public class StartCommandHandler
    {
        private readonly TelegramBotService _bot;

        public StartCommandHandler(TelegramBotService bot)
        {
            _bot = bot;
        }

        public async Task HandleAsync(Message message)
        {
            string introText = "Hello! I am your car insurance assistant bot. I will help you create your insurance policy step by step. Let’s begin!";
            await _bot.SendTextAsync(message.Chat.Id, introText);
        }
    }
}
