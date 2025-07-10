using Application.Interfaces;
using Application.Telegram.Commands;
using Domain.Entities;
using Infrastructure.Storage;
using Infrastructure.Telegram.Interface;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence.DbContext;

namespace Application.Telegram.Handlers
{
    public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Unit>
    {
        private readonly ITelegramBotService _botService;
        private readonly AppDbContext _context;
        private readonly ILogger<UploadDocumentCommandHandler> _logger;
        private readonly IFileStorageService _fileStorage;
        private readonly IUserStateService _stateService;

        public UploadDocumentCommandHandler(
            ITelegramBotService botService,
            AppDbContext context,
            ILogger<UploadDocumentCommandHandler> logger,
            IFileStorageService fileStorage,
            IUserStateService stateService)
        {
            _botService = botService;
            _context = context;
            _logger = logger;
            _fileStorage = fileStorage;
            _stateService = stateService;
        }

        public async Task<Unit> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
        {
          
            using var stream = new MemoryStream();
            await _botService.DownloadFileAsync(request.FileId, stream);

            var savedPath = await _fileStorage.SaveAsync(stream, request.FileName, request.TelegramUserId.ToString());

            var doc = new Document
            {
                TelegramUserId = request.TelegramUserId,
                FileId = request.FileId,
                FileName = request.FileName,
                FileType = request.FileType,
                LocalPath = savedPath
            };
            await _context.Documents.AddAsync(doc, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var currentStep = await _stateService.GetStepAsync(request.TelegramUserId);
            string nextStep;
            string userMessage;

            if (currentStep == "awaiting_passport")
            {
                nextStep = "awaiting_car_doc";
                userMessage = $"✅ Passport received.\n📤 Now please upload your car registration document.";
            }
            else if (currentStep == "awaiting_car_doc")
            {
                nextStep = "processing";
                userMessage = $"✅ Car document received.\n⏳ All documents received. Processing will begin shortly.";
            }
            else
            {
                nextStep = currentStep;
                userMessage = "⚠️ Unexpected document or invalid state.";
            }

            await _stateService.SetStepAsync(request.TelegramUserId, nextStep);

            await _botService.SendTextAsync(request.TelegramUserId,
                $"📎 Document '{request.FileName}' received and saved.\n\n{userMessage}");

            return Unit.Value;
        }
    }
}
