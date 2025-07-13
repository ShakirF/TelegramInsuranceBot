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
using Persistence.DbContext;

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

                if ((currentStep == UserStep.AwaitingPassport && request.FileType != "passport") ||
                    (currentStep == UserStep.AwaitingCarDoc && request.FileType != "car_registration"))
                {
                    var messageUnexpectedDocument = await _promptProvider.GetUnexpectedDocumentMessageAsync();

                    await _botService.SendTextAsync(request.TelegramUserId, messageUnexpectedDocument);
                    return Unit.Value;
                }
                var previousDocs = await _unitOfWork.Documents.Query()
                    .Where(d => d.TelegramUserId == request.TelegramUserId &&
                                d.FileType == request.FileType &&
                                d.IsActive)
                    .ToListAsync(cancellationToken);
               
                foreach (var predoc in previousDocs)
                {
                    predoc.IsActive = false;
                    _unitOfWork.Documents.Update(predoc);

                }

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

                await _unitOfWork.Documents.AddAsync(doc, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var messageSaved = await _promptProvider.GetDocumentSavedMessageAsync(request.FileName);

                await _botService.SendTextAsync(request.TelegramUserId, messageSaved);

                stream.Position = 0;
                var ocrResult = request.FileType switch
                {
                    "passport" => await _ocrService.ExtractPassportDataAsync(stream, request.FileName, cancellationToken),
                    "car_registration" => await _ocrService.ExtractVehicleDataAsync(stream, request.FileName, cancellationToken),
                    _ => throw new InvalidOperationException("Unsupported file type.")
                };

                var messageOcrDone = await _promptProvider.GetOcrDoneMessageAsync();
                await _botService.SendTextAsync(request.TelegramUserId, messageOcrDone);

                await _mediator.Send(new ParseOcrCommand
                {
                    DocumentId = doc.Id,
                    OcrJson = ocrResult
                }, cancellationToken);

                var nextStep = currentStep switch
                {
                    UserStep.AwaitingPassport => UserStep.AwaitingCarDoc,
                    UserStep.AwaitingCarDoc => UserStep.AwaitingConfirmation,
                    _ => currentStep
                };

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
    }


}
