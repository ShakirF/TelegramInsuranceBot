using Application.Interfaces;
using Application.Telegram.Commands;
using Application.Telegram.Handlers;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Telegram.Interface;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Handlers;

public class ConfirmPolicyCommandHandlerTests
{
    [Fact]
    public async Task Handle_AwaitingPriceConfirmation_SetsConfirmedAndSendsMessage()
    {
        // Arrange
        var mockState = new Mock<IUserStateService>();
        var mockBot = new Mock<ITelegramBotService>();
        var mockPrompt = new Mock<IPromptProvider>();
        var mockMediator = new Mock<IMediator>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockPolicyEventRepo = new Mock<IRepository<PolicyEvent>>();
        var mockErrorRepo = new Mock<IRepository<Error>>();

        var chatId = 1;

        // Setup state
        mockState.Setup(x => x.GetStepAsync(chatId))
                 .ReturnsAsync(UserStep.AwaitingPriceConfirmation);

        // Setup prompt
        mockPrompt.Setup(x => x.GetPolicyConfirmedMessageAsync())
                  .ReturnsAsync("✅ Confirmed!");

        // Setup unit of work
        mockUnitOfWork.Setup(x => x.PolicyEvents)
                      .Returns(mockPolicyEventRepo.Object);
        mockUnitOfWork.Setup(x => x.Errors)
                      .Returns(mockErrorRepo.Object);
        mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(1);

        var handler = new ConfirmPolicyCommandHandler(
            mockState.Object,
            mockBot.Object,
            Mock.Of<ILogger<ConfirmPolicyCommandHandler>>(),
            mockPrompt.Object,
            mockMediator.Object,
            mockUnitOfWork.Object);

        // Act
        await handler.Handle(new ConfirmPolicyCommand { ChatId = chatId }, CancellationToken.None);

        // Assert
        mockState.Verify(x => x.SetStepAsync(chatId, UserStep.Confirmed), Times.Once);
        mockBot.Verify(x => x.SendTextAsync(chatId, "✅ Confirmed!"), Times.Once);
        mockPolicyEventRepo.Verify(x => x.AddAsync(It.IsAny<PolicyEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockMediator.Verify(x => x.Send(It.IsAny<GeneratePolicyCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
