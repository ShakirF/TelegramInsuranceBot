using MediatR;

namespace Application.Telegram.Commands
{
    public class ParseOcrCommand : IRequest<Unit>
    {
        public int DocumentId { get; set; }
        public string OcrJson { get; set; } = null!;
    }
}
