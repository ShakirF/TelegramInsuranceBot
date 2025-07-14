using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Telegram.Interface
{
    public interface ITelegramBotService
    {
        Task SendTextAsync(long chatId, string message);
        Task SetWebhookAsync(string url);
        Task DownloadFileAsync(string fileId, Stream destination);
        Task SendDocumentAsync(long chatId, string filePath, string caption = "");
    }
}
