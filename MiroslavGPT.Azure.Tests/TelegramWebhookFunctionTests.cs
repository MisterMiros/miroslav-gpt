using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Tests.Core;
using Telegram.Bot.Types;

namespace MiroslavGPT.Azure.Tests;

public class TelegramWebhookFunctionTests
{
    private Fixture _fixture = null!;
    private Mock<ITelegramMessageHandler> _mockTelegramMessageHandler = null!;
    private Mock<ILogger<TelegramWebhookFunction>> _mockLogger = null!;
    private TelegramWebhookFunction _function = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customize(new AutoMoqCustomization());
        _mockTelegramMessageHandler = _fixture.Freeze<Mock<ITelegramMessageHandler>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<TelegramWebhookFunction>>>();
        _function = new(_mockTelegramMessageHandler.Object, _mockLogger.Object);
    }

    [Test]
    public async Task Run_ReturnsOk()
    {
        // Arrange
        var update = new Update();
        var req = _fixture.CreateMockHttpRequest(update);

        // Act
        var result = await _function.Run(req);

        // Assert
        result.Should().BeOfType<OkResult>();

        _mockTelegramMessageHandler.Verify(h => h.ProcessUpdateAsync(It.Is<Update>(u => u.Id == update.Id)), Times.Once());
        _mockTelegramMessageHandler.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Run_ReturnsSuccess_AndLogsError_WhenNoBody()
    {
        // Arrange
        var req = _fixture.CreateMockHttpRequest("");

        // Act
        var result = await _function.Run(req);

        // Assert
        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(StatusCodes.Status200OK);


        _mockLogger.VerifyLogError("Error processing webhook request");

        _mockTelegramMessageHandler.Verify(h => h.ProcessUpdateAsync(It.IsAny<Update>()), Times.Never);
        _mockTelegramMessageHandler.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Run_ReturnsSuccess_AngLogsError_WhenProcessThrowsException()
    {
        // Arrange
        var update = new Update();
        var req = _fixture.CreateMockHttpRequest(update);
        var ex = new Exception("Failed");

        _mockTelegramMessageHandler.Setup(h => h.ProcessUpdateAsync(It.IsAny<Update>()))
            .ThrowsAsync(ex);

        // Act
        var result = await _function.Run(req);

        // Assert
        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(StatusCodes.Status200OK);

        _mockLogger.VerifyLogError(ex, "Error processing webhook request");

        _mockTelegramMessageHandler.Verify(h => h.ProcessUpdateAsync(It.Is<Update>(u => u.Id == update.Id)), Times.Once());
        _mockTelegramMessageHandler.VerifyNoOtherCalls();
    }
}