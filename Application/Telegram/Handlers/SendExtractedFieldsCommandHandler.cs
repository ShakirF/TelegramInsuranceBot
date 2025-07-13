using Application.Telegram.Commands;
using Domain.Interfaces;
using Infrastructure.Telegram.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.DbContext;
using System.Text;

namespace Application.Telegram.Handlers
{
  public class SendExtractedFieldsCommandHandler : IRequestHandler<SendExtractedFieldsCommand, Unit>
{
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITelegramBotService _botService;

        public SendExtractedFieldsCommandHandler(ITelegramBotService botService, IUnitOfWork unitOfWork)
        {
            _botService = botService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(SendExtractedFieldsCommand request, CancellationToken cancellationToken)
    {
            var documents = await _unitOfWork.Documents.Query()
                .Where(d => d.TelegramUserId == request.ChatId && d.OcrRawJson != null && d.IsActive)
                .OrderBy(d => d.UploadedAt)
                .ToListAsync(cancellationToken);

            if (!documents.Any())
        {
            await _botService.SendTextAsync(request.ChatId, "⚠️ No extracted fields found.");
            return Unit.Value;
        }

        var sb = new StringBuilder();
        sb.AppendLine("📋 <b>Here is what I found in your documents</b>:\n");

        foreach (var doc in documents)
        {
            sb.AppendLine($"📌 <b>{doc.FileType.ToUpper()}</b>:");

                var fields = await _unitOfWork.ExtractedFields.Query()
                     .Where(f => f.DocumentId == doc.Id)
                     .ToListAsync(cancellationToken);

                if (fields.Count == 0)
            {
                sb.AppendLine("⚠️ No fields extracted.\n");
                continue;
            }

            foreach (var field in fields)
            {
                sb.AppendLine($"🔹 <b>{field.FieldName}</b>: {field.FieldValue}");
            }

            sb.AppendLine(); // boş sətr
        }

        sb.AppendLine("✅ If everything looks good, type <b>confirm</b>.");
        sb.AppendLine("🔁 Otherwise, re-upload the correct document.");

        await _botService.SendTextAsync(request.ChatId, sb.ToString());

        return Unit.Value;
    }
}

}
