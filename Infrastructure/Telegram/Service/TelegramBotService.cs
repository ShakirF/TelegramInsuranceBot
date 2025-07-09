using Infrastructure.Telegram.Interface;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Infrastructure.Telegram.Service
{
    public class TelegramBotService: ITelegramBotService
    {
        private readonly ITelegramBotClient _botClient;

        public TelegramBotService(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task SendTextAsync(long chatId, string message)
        {
            await _botClient.SendMessage(
                chatId: chatId,
                text: message,
                parseMode: ParseMode.Html
            );
        }

        public async Task SetWebhookAsync(string url)
        {
            await _botClient.DeleteWebhook();
            if (!string.IsNullOrWhiteSpace(url))
            {
                await _botClient.SetWebhook(url);
            }
        }
    }
}
