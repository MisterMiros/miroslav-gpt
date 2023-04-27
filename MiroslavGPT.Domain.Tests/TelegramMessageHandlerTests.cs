using MiroslavGPT.Domain.Factories;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Settings;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MiroslavGPT.Domain.Tests
{
    public class TelegramMessageHandlerTests
    {
        private Fixture _fixture;
        private Mock<ITelegramBotSettings> _mockSettings;
        private Mock<ITelegramClientFactory> _mockTelegramClientFactory;
        private Mock<ITelegramBotClient> _mockTelegramBotClient;
        private Mock<IBot> _mockBot;
        private TelegramMessageHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _mockSettings = _fixture.Freeze<Mock<ITelegramBotSettings>>();
            _mockTelegramBotClient = _fixture.Freeze<Mock<ITelegramBotClient>>();
            _mockTelegramClientFactory = _fixture.Freeze<Mock<ITelegramClientFactory>>();
            _mockTelegramClientFactory.Setup(f => f.CreateBotClient(It.IsAny<string>()))
                .Returns(_mockTelegramBotClient.Object);
            _mockBot = _fixture.Freeze<Mock<IBot>>();
            _handler = _fixture.Create<TelegramMessageHandler>();
        }

        [Test]
        public async Task ProcessUpdateAsync_ShouldSkip_WhenUpdateEmpty()
        {
            // Arrange
            // Act
            await _handler.ProcessUpdateAsync(null);

            // Assert
            _mockBot.Verify(b => b.ProcessCommandAsync(It.IsAny<Update>()), Times.Never);
            _mockTelegramBotClient.Verify(c => c.MakeRequestAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ProcessUpdateAsync_ShouldSkip_WhenUpdateMessageEmpty()
        {
            // Arrange
            var update = _fixture.Build<Update>().With(r => r.Message, (Message)null).Create();

            // Act
            await _handler.ProcessUpdateAsync(update);

            // Assert
            _mockBot.Verify(b => b.ProcessCommandAsync(It.IsAny<Update>()), Times.Never);
            _mockTelegramBotClient.Verify(c => c.MakeRequestAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public async Task ProcessUpdateAsync_ShouldSkip_WhenUpdateMessageTextEmpty(string text)
        {
            // Arrange
            var update = _fixture.Build<Update>()
                .With(r => r.Message, _fixture.Build<Message>().With(m => m.Text, text).Create())
                .Create();

            // Act
            await _handler.ProcessUpdateAsync(update);

            // Assert
            _mockBot.Verify(b => b.ProcessCommandAsync(It.IsAny<Update>()), Times.Never);
            _mockTelegramBotClient.Verify(c => c.MakeRequestAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ProcessUpdateAsync_ShouldSkip_WhenNotACommand()
        {
            // Arrange
            var update = _fixture.Create<Update>();

            // Act
            await _handler.ProcessUpdateAsync(update);

            // Assert
            _mockBot.Verify(b => b.ProcessCommandAsync(It.IsAny<Update>()), Times.Never);
            _mockTelegramBotClient.Verify(c => c.MakeRequestAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestCase(ChatType.Supergroup)]
        [TestCase(ChatType.Channel)]
        [TestCase(ChatType.Sender)]
        public async Task ProcessUpdateAsync_ShouldSkip_WhenNotSupportedGroup(ChatType chatType)
        {
            // Arrange
            var botName = "thebot";
            var update = _fixture.Create<Update>();
            update.Message.Text = "/command";
            update.Message.Chat.Type = chatType;

            _mockSettings.Setup(s => s.TelegramBotUsername)
                .Returns(botName);

            // Act
            await _handler.ProcessUpdateAsync(update);

            // Assert
            _mockBot.Verify(b => b.ProcessCommandAsync(It.IsAny<Update>()), Times.Never);
            _mockTelegramBotClient.Verify(c => c.MakeRequestAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestCase("/command")]
        [TestCase("/command@wrongname")]
        [TestCase("/command@")]
        [TestCase("/command argument")]
        [TestCase("/command@wrongname argument")]
        [TestCase("/command@ argument")]
        public async Task ProcessUpdateAsync_ShouldSkip_WhenGroupChatWithoutBotName(string text)
        {
            // Arrange
            var botName = "thebot";
            var update = _fixture.Create<Update>();
            update.Message.Text = text;
            update.Message.Chat.Type = Telegram.Bot.Types.Enums.ChatType.Group;

            _mockSettings.Setup(s => s.TelegramBotUsername)
                .Returns(botName);

            // Act
            await _handler.ProcessUpdateAsync(update);

            // Assert
            _mockBot.Verify(b => b.ProcessCommandAsync(It.IsAny<Update>()), Times.Never);
            _mockTelegramBotClient.Verify(c => c.MakeRequestAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ProcessUpdateAsync_ShouldSendMessage_WhenSuccessfullyProcessed()
        {
            // Arrange
            var text = "/command";
            var update = _fixture.Create<Update>();
            var response = _fixture.Create<string>();
            update.Message.Text = text;
            update.Message.Chat.Type = ChatType.Private;

            _mockBot.Setup(b => b.ProcessCommandAsync(update)).ReturnsAsync(response);

            // Act
            await _handler.ProcessUpdateAsync(update);

            // Assert
            _mockTelegramBotClient.Verify(c => c.MakeRequestAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockTelegramBotClient.Verify(c => c.MakeRequestAsync(It.Is<SendMessageRequest>(r =>
            r.ChatId == update.Message.Chat.Id
            && r.Text == response
            && r.ReplyToMessageId == update.Message.MessageId
            && r.DisableWebPagePreview == true
            && r.ParseMode == ParseMode.Markdown), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ProcessUpdateAsync_ShouldSendMessage_WnenSuccessfullyProcessed_AndGroupChat()
        {
            // Arrange
            var text = "/command@botname";
            var update = _fixture.Create<Update>();
            var response = _fixture.Create<string>();
            update.Message.Text = text;
            update.Message.Chat.Type = ChatType.Group;

            _mockSettings.Setup(s => s.TelegramBotUsername)
                .Returns("botname");

            _mockBot.Setup(b => b.ProcessCommandAsync(update))
                .ReturnsAsync(response);

            // Act
            await _handler.ProcessUpdateAsync(update);

            // Assert
            _mockTelegramBotClient.Verify(c => c.MakeRequestAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockTelegramBotClient.Verify(c => c.MakeRequestAsync(It.Is<SendMessageRequest>(r =>
            r.ChatId == update.Message.Chat.Id
            && r.Text == response
            && r.ReplyToMessageId == update.Message.MessageId
            && r.DisableWebPagePreview == true
            && r.ParseMode == ParseMode.Markdown), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ProcessUpdateAsync_ShouldSendMessage_OnException()
        {
            // Arrange
            var text = "/command";
            var update = _fixture.Create<Update>();
            update.Message.Text = text;
            update.Message.Chat.Type = ChatType.Private;

            var ex = new Exception("Failed");

            _mockBot.Setup(b => b.ProcessCommandAsync(update)).ThrowsAsync(ex);

            // Act
            // Assert
            Assert.ThrowsAsync<Exception>(async () => await _handler.ProcessUpdateAsync(update))
                .Should()
                .NotBeNull()
                .And
                .Be(ex);

            _mockTelegramBotClient.Verify(c => c.MakeRequestAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockTelegramBotClient.Verify(c => c.MakeRequestAsync(It.Is<SendMessageRequest>(r =>
            r.ChatId == update.Message.Chat.Id
            && r.Text == "Error handling the command"
            && r.ReplyToMessageId == update.Message.MessageId
            && r.DisableWebPagePreview == true
            && r.ParseMode == ParseMode.Markdown), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
