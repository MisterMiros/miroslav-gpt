using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MiroslavGPT.Domain.Interfaces;
using Newtonsoft.Json;
using System.Net;
using Microsoft.Extensions.Logging;
using MiroslavGPT.AWS.Settings;
using MiroslavGPT.Tests.Core;
using Update = Telegram.Bot.Types.Update;

namespace MiroslavGPT.AWS.Tests;

public class TelegramWebhookFunctionTests
{
    private Fixture _fixture;
    private Mock<ITelegramMessageHandler> _mockTelegramMessageHandler;
    private Mock<ILogger<TelegramWebhookFunction>> _mockLogger;
    private Mock<ILambdaContext> _mockContext;
    private TelegramWebhookFunction _function;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customize(new AutoMoqCustomization());
        _mockTelegramMessageHandler = _fixture.Freeze<Mock<ITelegramMessageHandler>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<TelegramWebhookFunction>>>();
        _mockContext = _fixture.Freeze<Mock<ILambdaContext>>();
        _function = new TelegramWebhookFunction(_mockTelegramMessageHandler.Object, _mockLogger.Object);
    }

    [Test, AutoData]
    public void Constructor_WorksFine(AmazonSettings settings)
    {
        // Arrange
        Environment.SetEnvironmentVariable("AWS_REGION", settings.RegionName);
        Environment.SetEnvironmentVariable("TELEGRAM_BOT_USERNAME", settings.TelegramBotUsername);
        Environment.SetEnvironmentVariable("TELEGRAM_BOT_TOKEN", settings.TelegramBotToken);
        Environment.SetEnvironmentVariable("SECRET_KEY", settings.SecretKey);
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", settings.OpenAiApiKey);
        Environment.SetEnvironmentVariable("MAX_TOKENS", settings.MaxTokens.ToString());
        Environment.SetEnvironmentVariable("DYNAMODB_USERS_TABLE_NAME", settings.UsersTableName);
        Environment.SetEnvironmentVariable("DYNAMODB_THREAD_TABLE_NAME", settings.ThreadTableName);
        Environment.SetEnvironmentVariable("THREAD_LENGTH_LIMIT", settings.ThreadLengthLimit.ToString());
        // Act
        // Assert
        Assert.DoesNotThrow(() => new TelegramWebhookFunction());
    }

    [Test]
    public async Task Run_ReturnsOk()
    {
        // Arrange
        var update = new Update();
        var req = _fixture.Create<APIGatewayProxyRequest>();
        req.Body = JsonConvert.SerializeObject(update);

        // Act
        var result = await _function.FunctionHandler(req, _mockContext.Object);

        // Assert
        result.StatusCode.Should().Be((int)HttpStatusCode.OK);

        _mockTelegramMessageHandler.Verify(h => h.ProcessUpdateAsync(It.Is<Update>(u => u.Id == update.Id)), Times.Once());
        _mockTelegramMessageHandler.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Run_ReturnsOk_WhenNoBody()
    {
        // Arrange
        var req = _fixture.Create<APIGatewayProxyRequest>();
        req.Body = "";

        // Act
        var result = await _function.FunctionHandler(req, _mockContext.Object);

        // Assert
        result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        _mockLogger.VerifyLogError("Error processing webhook request");

        _mockTelegramMessageHandler.Verify(h => h.ProcessUpdateAsync(It.IsAny<Update>()), Times.Never);
        _mockTelegramMessageHandler.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Run_ReturnsOk_WhenProcessThrowsException()
    {
        // Arrange
        var update = new Update();
        var req = _fixture.Create<APIGatewayProxyRequest>();
        req.Body = JsonConvert.SerializeObject(update);
        var ex = new Exception("Failed");

        _mockTelegramMessageHandler.Setup(h => h.ProcessUpdateAsync(It.IsAny<Update>()))
            .ThrowsAsync(ex);

        // Act
        var result = await _function.FunctionHandler(req, _mockContext.Object);

        // Assert
        result.StatusCode.Should().Be((int)HttpStatusCode.OK);

        _mockLogger.VerifyLogError(ex, "Error processing webhook request");

        _mockTelegramMessageHandler.Verify(h => h.ProcessUpdateAsync(It.Is<Update>(u => u.Id == update.Id)), Times.Once());
        _mockTelegramMessageHandler.VerifyNoOtherCalls();
    }
}