using Application.Interfaces;
using Application.Telegram.Commands;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Storage;
using Infrastructure.Telegram.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Persistence.DbContext;
using Shared.Utilities;

namespace Application.Telegram.Handlers
{
    public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Unit>
    {
        private readonly ITelegramBotService _botService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UploadDocumentCommandHandler> _logger;
        private readonly IFileStorageService _fileStorage;
        private readonly IUserStateService _stateService;
        private readonly ICustomOcrService _ocrService;
        private readonly IMediator _mediator;
        private readonly IPromptProvider _promptProvider;

        public UploadDocumentCommandHandler(
            ITelegramBotService botService,
            IUnitOfWork unitOfWork,
            ILogger<UploadDocumentCommandHandler> logger,
            IFileStorageService fileStorage,
            IUserStateService stateService,
            ICustomOcrService ocrService,
            IMediator mediator,
            IPromptProvider promptProvider)
        {
            _botService = botService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _fileStorage = fileStorage;
            _stateService = stateService;
            _ocrService = ocrService;
            _mediator = mediator;
            _promptProvider = promptProvider;
        }

        public async Task<Unit> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var currentStep = await _stateService.GetStepAsync(request.TelegramUserId);

                if (!IsValidFileTypeForStep(currentStep, request.FileType))
                {
                    await _botService.SendTextAsync(request.TelegramUserId, await _promptProvider.GetUnexpectedDocumentMessageAsync());
                    return Unit.Value;
                }

                await DeactivatePreviousDocuments(request.TelegramUserId, request.FileType, cancellationToken);

                using var stream = new MemoryStream();
                await _botService.DownloadFileAsync(request.FileId, stream);

                var hash = FileHashHelper.ComputeSHA256(stream);

                var isDuplicate = await _unitOfWork.Documents.Query()
                    .AnyAsync(d => d.TelegramUserId == request.TelegramUserId && d.ContentHash == hash && d.IsActive, cancellationToken);

                if (isDuplicate)
                {
                    await _botService.SendTextAsync(request.TelegramUserId, "⚠️ You've already uploaded this file.");
                    return Unit.Value;
                }
                var savedPath = await _fileStorage.SaveAsync(stream, request.FileName, request.TelegramUserId.ToString());

                var user = await _unitOfWork.Users.Query()
                    .FirstOrDefaultAsync(u => u.TelegramUserId == request.TelegramUserId, cancellationToken);
                
                if (user != null)
                {
                    user.UploadRetryCount++;
                    _unitOfWork.Users.Update(user);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                else 
                {
                    _logger.LogWarning("User not found for TelegramUserId={TelegramUserId}", request.TelegramUserId);
                    await _botService.SendTextAsync(request.TelegramUserId, "❗ Please start the bot with /start first.");
                    return Unit.Value;
                }

                var doc = new Document
                {
                    TelegramUserId = request.TelegramUserId,
                    FileId = request.FileId,
                    FileName = request.FileName,
                    FileType = request.FileType,
                    LocalPath = savedPath,
                    UploadedAt = DateTime.UtcNow,
                    UserId = user.Id,
                    ContentHash = hash,
                };

                await _unitOfWork.Documents.AddAsync(doc, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var messageSaved = await _promptProvider.GetDocumentSavedMessageAsync(request.FileName);

                await _botService.SendTextAsync(request.TelegramUserId, messageSaved);

                stream.Position = 0;
                var ocrResult = await RunOcrAsync(request.FileType, stream, request.FileName, cancellationToken);

                var messageOcrDone = await _promptProvider.GetOcrDoneMessageAsync();
                await _botService.SendTextAsync(request.TelegramUserId, messageOcrDone);

                await _mediator.Send(new ParseOcrCommand
                {
                    DocumentId = doc.Id,
                    OcrJson = ocrResult
                }, cancellationToken);

                var nextStep = GetNextStep(currentStep);

                await _stateService.SetStepAsync(request.TelegramUserId, nextStep);

                if (nextStep == UserStep.AwaitingCarDoc)
                {
                    var messagePassportReceived = await _promptProvider.GetPassportPromptMessageAsync();
                    await _botService.SendTextAsync(request.TelegramUserId, messagePassportReceived);
                }
                else if (nextStep == UserStep.AwaitingConfirmation)
                {
                    var messageCarDocReceived = await _promptProvider.GetCarDocPromptMessageAsync(request.FileName);
                    await _botService.SendTextAsync(request.TelegramUserId, messageCarDocReceived);

                    await _mediator.Send(new SendExtractedFieldsCommand
                    {
                        ChatId = request.TelegramUserId,
                        DocumentId = doc.Id
                    });

                }

                return Unit.Value;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error during OCR for user {UserId}", request.TelegramUserId);

                var messageOcrError = await _promptProvider.GetOcrErrorMessageAsync();
                await _botService.SendTextAsync(request.TelegramUserId, messageOcrError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in UploadDocumentCommandHandler for user {UserId}", request.TelegramUserId);

                var messageUnexpectedError = await _promptProvider.GetUnexpectedErrorMessageAsync();
                await _botService.SendTextAsync(request.TelegramUserId, messageUnexpectedError);
            }

            return Unit.Value;
        }

        private bool IsValidFileTypeForStep(UserStep step, string fileType) =>
            (step == UserStep.AwaitingPassport && fileType == "passport") ||
            (step == UserStep.AwaitingCarDoc && fileType == "car_registration");

        private async Task DeactivatePreviousDocuments(long userId, string fileType, CancellationToken cancellationToken)
        {
            var previousDocs = await _unitOfWork.Documents.Query()
                .Where(d => d.TelegramUserId == userId && d.FileType == fileType && d.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var doc in previousDocs)
            {
                doc.IsActive = false;
                _unitOfWork.Documents.Update(doc);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private Task<string> RunOcrAsync(string type, Stream stream, string name, CancellationToken cancellationToken) =>
        type switch
        {
            "passport" => _ocrService.ExtractPassportDataAsync(stream, name, cancellationToken),
            "car_registration" => _ocrService.ExtractVehicleDataAsync(stream, name, cancellationToken),
            _ => throw new InvalidOperationException("Unsupported file type")
        };

        private UserStep GetNextStep(UserStep currentStep) =>
        currentStep switch
        {
            UserStep.AwaitingPassport => UserStep.AwaitingCarDoc,
            UserStep.AwaitingCarDoc => UserStep.AwaitingConfirmation,
            _ => currentStep
        };

    }




}
