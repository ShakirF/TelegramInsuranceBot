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
            var fields = await _unitOfWork.ExtractedFields.Query()
                .Include(f => f.Document)
                .Where(f => f.Document.TelegramUserId == request.ChatId && f.Document.IsActive)
                .ToListAsync(cancellationToken);

            if (!fields.Any())
            {
                await _botService.SendTextAsync(request.ChatId, "❌ No extracted fields found in the document.");
                return Unit.Value;
            }

            var passportFields = fields.Where(f => f.Document.FileType == "passport").ToList();
            var carFields = fields.Where(f => f.Document.FileType == "car_registration").ToList();

            var message = new StringBuilder("📄 Here is what I found in your documents:\n");

            if (passportFields.Any())
            {
                message.AppendLine("\n📘 *PASSPORT:*");
                foreach (var field in passportFields)
                    message.AppendLine($"`{field.FieldName}`: {field.FieldValue}");
            }

            if (carFields.Any())
            {
                message.AppendLine("\n🚗 *CAR_REGISTRATION:*");
                foreach (var field in carFields)
                    message.AppendLine($"`{field.FieldName}`: {field.FieldValue}");
            }

            message.AppendLine("\n✅ If everything looks good, type *confirm*.");
            message.AppendLine("🌀 Otherwise, re-upload the correct document.");

            await _botService.SendTextAsync(request.ChatId, message.ToString());

            return Unit.Value;
        }
    }
}

