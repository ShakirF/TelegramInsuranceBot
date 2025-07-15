using Application.Interfaces;
using Application.Telegram.Commands;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Telegram.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Telegram.Handlers
{
    public class SimulateOcrCommandHandler : IRequestHandler<SimulateOcrCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITelegramBotService _bot;
        private readonly IMediator _mediator;
        private readonly IAdminService _admin;

        public SimulateOcrCommandHandler(
            IUnitOfWork unitOfWork,
            ITelegramBotService bot,
            IMediator mediator,
            IAdminService admin)
        {
            _unitOfWork = unitOfWork;
            _bot = bot;
            _mediator = mediator;
            _admin = admin;
        }

        public async Task<Unit> Handle(SimulateOcrCommand request, CancellationToken cancellationToken)
        {
            if (!_admin.IsAdmin(request.ChatId))
            {
                await _bot.SendTextAsync(request.ChatId, "❌ You are not authorized to use this command.");
                return Unit.Value;
            }

            var doc = await _unitOfWork.Documents.Query()
                .Where(d => d.TelegramUserId == request.ChatId && d.IsActive)
                .OrderByDescending(d => d.UploadedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (doc == null)
            {
                await _bot.SendTextAsync(request.ChatId, "⚠️ No active document found.");
                return Unit.Value;
            }

            var mockFields = new List<ExtractedField>
        {
            new() { DocumentId = doc.Id, FieldName = "Full Name", FieldValue = "Test User" },
            new() { DocumentId = doc.Id, FieldName = "Passport Number", FieldValue = "ABC123456" },
            new() { DocumentId = doc.Id, FieldName = "Date of Birth", FieldValue = "1990-01-01" },
        };

            await _unitOfWork.ExtractedFields.AddRangeAsync(mockFields, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _mediator.Send(new SendExtractedFieldsCommand
            {
                ChatId = request.ChatId,
                DocumentId = doc.Id
            }, cancellationToken);

            await _bot.SendTextAsync(request.ChatId, "✅ Simulated OCR result injected.");
            return Unit.Value;
        }
    }
}
