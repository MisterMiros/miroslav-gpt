using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Tests.Core;
using Newtonsoft.Json;
using System.Net;
using Update = Telegram.Bot.Types.Update;

namespace MiroslavGPT.AWS.Tests
{
    public class TelegramWebhookFunctionTests
    {
        private Fixture _fixture;
        private Mock<ITelegramMessageHandler> _mockTelegramMessageHandler;
        private Mock<ILambdaLogger> _mockLogger;
        private Mock<ILambdaContext> _mockContext;
        private TelegramWebhookFunction _function;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Customizations.Add(new RecursionDepthCustomization(1));
            _fixture.Customize(new AutoMoqCustomization());
            _mockTelegramMessageHandler = _fixture.Freeze<Mock<ITelegramMessageHandler>>();
            _mockLogger = _fixture.Freeze<Mock<ILambdaLogger>>();
            _mockContext= _fixture.Freeze<Mock<ILambdaContext>>();
            _mockContext.Setup(c => c.Logger).Returns(_mockLogger.Object);
            _function = new TelegramWebhookFunction(_mockTelegramMessageHandler.Object);
        }

        [Test]
        public void Constructor_WorksFine()
        {
            // Arrange
            // Act
            // Assert
            Assert.DoesNotThrow(() => new TelegramWebhookFunction());
            
        }

        [Test]
        public async Task Run_ReturnsOk()
        {
            // Arrange
            var update = _fixture.Create<Update>();
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
        public async Task Run_ReturnsInternalError_WhenNoBody()
        {
            // Arrangevar update = _fixture.Create<Update>();
            var req = _fixture.Create<APIGatewayProxyRequest>();
            req.Body = "";

            // Act
            var result = await _function.FunctionHandler(req, _mockContext.Object);

            // Assert
            result.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

            _mockLogger.Verify(l => l.LogError(It.Is<string>(s => s.StartsWith($"Error processing webhook request: Newtonsoft.Json.JsonReaderException: Error reading JObject from JsonReader"))), Times.Once);

            _mockTelegramMessageHandler.Verify(h => h.ProcessUpdateAsync(It.IsAny<Update>()), Times.Never);
            _mockTelegramMessageHandler.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Run_ReturnsInternalError_WhenProccessThrowsException()
        {
            // Arrange
            var update = _fixture.Create<Update>();
            var req = _fixture.Create<APIGatewayProxyRequest>();
            req.Body = JsonConvert.SerializeObject(update);
            var ex = new Exception("Failed");

            _mockTelegramMessageHandler.Setup(h => h.ProcessUpdateAsync(It.IsAny<Update>()))
                .ThrowsAsync(ex);

            // Act
            var result = await _function.FunctionHandler(req, _mockContext.Object);

            // Assert
            result.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

            _mockLogger.Verify(l => l.LogError($"Error processing webhook request: {ex}"), Times.Once);

            _mockTelegramMessageHandler.Verify(h => h.ProcessUpdateAsync(It.Is<Update>(u => u.Id == update.Id)), Times.Once());
            _mockTelegramMessageHandler.VerifyNoOtherCalls();
        }
    }
}
