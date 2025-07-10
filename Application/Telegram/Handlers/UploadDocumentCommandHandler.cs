using Application.Interfaces;
using Application.Telegram.Commands;
using Domain.Entities;
using Domain.Enums;
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
        private readonly ICustomOcrService _ocrService;
        private readonly IMediator _mediator;

        public UploadDocumentCommandHandler(
            ITelegramBotService botService,
            AppDbContext context,
            ILogger<UploadDocumentCommandHandler> logger,
            IFileStorageService fileStorage,
            IUserStateService stateService,
            ICustomOcrService ocrService,
            IMediator mediator)
        {
            _botService = botService;
            _context = context;
            _logger = logger;
            _fileStorage = fileStorage;
            _stateService = stateService;
            _ocrService = ocrService;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
        {
            try
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
                    LocalPath = savedPath,
                    UploadedAt = DateTime.UtcNow
                };
                await _context.Documents.AddAsync(doc, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                var currentStep = await _stateService.GetStepAsync(request.TelegramUserId);
                UserStep nextStep;
                string userMessage;

                if (currentStep == UserStep.AwaitingPassport)
                {
                    nextStep = UserStep.AwaitingCarDoc;
                    userMessage = $"✅ Passport received.\n📤 Now please upload your car registration document.";
                }
                else if (currentStep == UserStep.AwaitingCarDoc)
                {
                    nextStep = UserStep.AwaitingConfirmation;
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

                stream.Position = 0;
                var ocrResult = request.FileType switch
                {
                    "passport" => await _ocrService.ExtractPassportDataAsync(stream, request.FileName, cancellationToken),
                    "car_registration" => await _ocrService.ExtractVehicleDataAsync(stream, request.FileName, cancellationToken),
                    _ => throw new InvalidOperationException("Unsupported file type for OCR.")
                };

                _logger.LogInformation("📄 OCR Result for {FileName}: {OcrResult}", request.FileName, ocrResult);

                await _botService.SendTextAsync(request.TelegramUserId, "🧠 OCR processing complete.");

                await _mediator.Send(new ParseOcrCommand
                {
                    DocumentId = doc.Id,
                    OcrJson = ocrResult
                }, cancellationToken);

                await _stateService.SetStepAsync(request.TelegramUserId, UserStep.AwaitingConfirmation);

                await _mediator.Send(new SendExtractedFieldsCommand
                {
                    ChatId = request.TelegramUserId,
                    DocumentId = doc.Id
                });

                await _botService.SendTextAsync(request.TelegramUserId, $"📎 Document '{request.FileName}' saved and OCR processed. Please confirm the extracted data.");

                return Unit.Value;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error during OCR for user {UserId}", request.TelegramUserId);
                await _botService.SendTextAsync(request.TelegramUserId, "❗ Failed to contact OCR service. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in UploadDocumentCommandHandler for user {UserId}", request.TelegramUserId);
                await _botService.SendTextAsync(request.TelegramUserId, "❗ An unexpected error occurred. Please try again later.");
            }

            return Unit.Value;
        }
    }

}
