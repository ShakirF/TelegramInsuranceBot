using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FluentAssertions;
using MockQueryable;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Services
{
    public class UserStateServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly UserStateService _service;

        public UserStateServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _service = new UserStateService(_mockUow.Object);
        }

        [Fact]
        public async Task GetStepAsync_ReturnsStart_IfNoStateFound()
        {
            // Arrange
            var data = new List<UserState>().AsQueryable().BuildMockDbSet();
            _mockUow.Setup(u => u.UserStates.Query())
                    .Returns(data.Object);

            // Act
            var step = await _service.GetStepAsync(123);

            // Assert
            step.Should().Be(UserStep.Start);
        }

        [Fact]
        public async Task SetStepAsync_AddsNewState_IfNotExists()
        {
            var list = new List<UserState>();
            var data = list.AsQueryable().BuildMockDbSet();

            _mockUow.Setup(u => u.UserStates.Query()).Returns(data.Object);

            _mockUow.Setup(u => u.UserStates.AddAsync(It.IsAny<UserState>(), default))
                    .Callback<UserState, CancellationToken>((s, _) => list.Add(s));

            await _service.SetStepAsync(123, UserStep.AwaitingPassport);

            list.Should().ContainSingle();
            list[0].CurrentStep.Should().Be(UserStep.AwaitingPassport);
        }

    }
}
