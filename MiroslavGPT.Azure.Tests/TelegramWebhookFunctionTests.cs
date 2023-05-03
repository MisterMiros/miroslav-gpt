using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Tests.Core;
using Telegram.Bot.Types;

namespace MiroslavGPT.Azure.Tests;

public class TelegramWebhookFunctionTests
{
    private Fixture _fixture;
    private Mock<ITelegramMessageHandler> _mockTelegramMessageHandler;
    private Mock<ILogger<TelegramWebhookFunction>> _mockLogger;
    private TelegramWebhookFunction _function;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customize(new AutoMoqCustomization());
        _mockTelegramMessageHandler = _fixture.Freeze<Mock<ITelegramMessageHandler>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<TelegramWebhookFunction>>>();
        _function = new TelegramWebhookFunction(_mockTelegramMessageHandler.Object);
    }

    [Test]
    public async Task Run_ReturnsOk()
    {
        // Arrange
        var update = new Update();
        var req = _fixture.CreateMockHttpRequest(update);

        // Act
        var result = await _function.Run(req, _mockLogger.Object);

        // Assert
        result.Should().BeOfType<OkResult>();

        _mockTelegramMessageHandler.Verify(h => h.ProcessUpdateAsync(It.Is<Update>(u => u.Id == update.Id)), Times.Once());
        _mockTelegramMessageHandler.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Run_ReturnsInternalError_WhenNoBody()
    {
        // Arrange
        var req = _fixture.CreateMockHttpRequest("");

        // Act
        var result = await _function.Run(req, _mockLogger.Object);

        // Assert
        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);


        _mockLogger.VerifyLogError($"Error processing webhook request: Error reading JObject from JsonReader. Current JsonReader item is not an object: String. Path '', line 1, position 2.");

        _mockTelegramMessageHandler.Verify(h => h.ProcessUpdateAsync(It.IsAny<Update>()), Times.Never);
        _mockTelegramMessageHandler.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Run_ReturnsInternalError_WhenProcessThrowsException()
    {
        // Arrange
        var update = new Update();
        var req = _fixture.CreateMockHttpRequest(update);
        var ex = new Exception("Failed");

        _mockTelegramMessageHandler.Setup(h => h.ProcessUpdateAsync(It.IsAny<Update>()))
            .ThrowsAsync(ex);

        // Act
        var result = await _function.Run(req, _mockLogger.Object);

        // Assert
        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        _mockLogger.VerifyLogError($"Error processing webhook request: {ex.Message}");

        _mockTelegramMessageHandler.Verify(h => h.ProcessUpdateAsync(It.Is<Update>(u => u.Id == update.Id)), Times.Once());
        _mockTelegramMessageHandler.VerifyNoOtherCalls();
    }
}