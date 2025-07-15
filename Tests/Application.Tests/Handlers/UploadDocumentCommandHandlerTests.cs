using Application.Interfaces;
using Application.Telegram.Commands;
using Application.Telegram.Handlers;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Storage;
using Infrastructure.Telegram.Interface;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Handlers
{
    public class UploadDocumentCommandHandlerTests
    {
        private readonly Mock<ITelegramBotService> _botMock = new();
        private readonly Mock<IUserStateService> _stateServiceMock = new();
        private readonly Mock<IFileStorageService> _storageMock = new();
        private readonly Mock<ICustomOcrService> _ocrMock = new();
        private readonly Mock<IUnitOfWork> _uowMock = new();
        private readonly Mock<IMediator> _mediatorMock = new();
        private readonly Mock<IPromptProvider> _msgMock = new();

        private readonly UploadDocumentCommandHandler _handler;

        public UploadDocumentCommandHandlerTests()
        {
            _handler = new UploadDocumentCommandHandler(
                _botMock.Object, _uowMock.Object, Mock.Of<ILogger<UploadDocumentCommandHandler>>(),
                _storageMock.Object, _stateServiceMock.Object,
                _ocrMock.Object, _mediatorMock.Object, _msgMock.Object);
        }

        [Fact]
        public async Task Handle_InvalidStep_ShowsWarning()
        {
            var cmd = new UploadDocumentCommand
            {
                TelegramUserId = 1,
                FileName = "test.jpg",
                FileId = "abc123",
                FileType = "car_registration"
            };

            _stateServiceMock.Setup(x => x.GetStepAsync(1)).ReturnsAsync(UserStep.AwaitingPassport);
            _msgMock.Setup(x => x.GetUnexpectedDocumentMessageAsync()).ReturnsAsync("Unexpected!");

            var result = await _handler.Handle(cmd, default);

            _botMock.Verify(x => x.SendTextAsync(1, "Unexpected!"), Times.Once);
        }
    }

}
