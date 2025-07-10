using MediatR;

namespace Application.Telegram.Commands
{
    public class UploadDocumentCommand : IRequest<Unit>
    {
        public long TelegramUserId { get; set; }
        public string FileId { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string FileType { get; set; } = null!; 
    }
}
