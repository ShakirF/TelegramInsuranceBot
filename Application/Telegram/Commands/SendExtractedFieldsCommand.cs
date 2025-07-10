using MediatR;

namespace Application.Telegram.Commands
{

    public class SendExtractedFieldsCommand : IRequest<Unit>
    {
        public long ChatId { get; set; }
        public int DocumentId { get; set; }
    }
}
