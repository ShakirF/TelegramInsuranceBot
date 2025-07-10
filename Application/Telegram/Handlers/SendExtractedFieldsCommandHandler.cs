using Application.Telegram.Commands;
using Infrastructure.Telegram.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.DbContext;
using System.Text;

namespace Application.Telegram.Handlers
{
    public class SendExtractedFieldsCommandHandler : IRequestHandler<SendExtractedFieldsCommand, Unit>
    {
        private readonly AppDbContext _context;
        private readonly ITelegramBotService _bot;

        public SendExtractedFieldsCommandHandler(AppDbContext context, ITelegramBotService bot)
        {
            _context = context;
            _bot = bot;
        }

        public async Task<Unit> Handle(SendExtractedFieldsCommand request, CancellationToken cancellationToken)
        {
            var fields = await _context.ExtractedFields
                .Where(x => x.DocumentId == request.DocumentId)
                .ToListAsync(cancellationToken);

            if (fields.Count == 0)
            {
                await _bot.SendTextAsync(request.ChatId, "⚠️ No extracted fields found.");
                return Unit.Value;
            }

            var sb = new StringBuilder();
            sb.AppendLine("📋 <b>Here is what I found in your document</b>:\n");

            foreach (var field in fields)
            {
                sb.AppendLine($"🔹 <b>{field.FieldName}</b>: {field.FieldValue} ({field.Confidence:P0})");
            }

            sb.AppendLine("\n✅ If everything looks good, type <b>confirm</b>.");
            sb.AppendLine("🔁 Otherwise, re-upload the correct document.");

            await _bot.SendTextAsync(request.ChatId, sb.ToString());

            return Unit.Value;
        }
    }
}
