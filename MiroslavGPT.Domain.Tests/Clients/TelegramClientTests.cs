using MiroslavGPT.Domain.Clients;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MiroslavGPT.Domain.Tests.Clients;

[TestFixture]
public class TelegramClientTests
{
    private Fixture _fixture;
    private Mock<ITelegramBotClient> _mockTelegramBotClient;
    private TelegramClient _telegramClient;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _mockTelegramBotClient = _fixture.Freeze<Mock<ITelegramBotClient>>();
        _telegramClient = _fixture.Create<TelegramClient>();
    }

    [Test, AutoData]
    public async Task SendTextMessageAsync_Works(long chatId, string text, int replyToMessageId)
    {
        // Arrange
        var message = _fixture.Create<Message>();
        _mockTelegramBotClient.Setup(c => c.MakeRequestAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        // Act
        var result = await _telegramClient.SendTextMessageAsync(chatId, text, replyToMessageId);
        result.Should().Be(message);

        _mockTelegramBotClient.Verify(c => c.MakeRequestAsync(It.Is<SendMessageRequest>(r =>
            r.ChatId == chatId
            && r.Text == text
            && r.ReplyToMessageId == replyToMessageId
            && r.DisableWebPagePreview == true
            && r.ParseMode == ParseMode.Markdown), It.IsAny<CancellationToken>()), Times.Once);
        _mockTelegramBotClient.VerifyNoOtherCalls();
    }
    
    [Test, AutoData]
    public async Task SendTextMessageAsync_Works_WhenNotReply(long chatId, string text)
    {
        // Arrange
        var message = _fixture.Create<Message>();
        _mockTelegramBotClient.Setup(c => c.MakeRequestAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        // Act
        var result = await _telegramClient.SendTextMessageAsync(chatId, text, null);
        result.Should().Be(message);

        _mockTelegramBotClient.Verify(c => c.MakeRequestAsync(It.Is<SendMessageRequest>(r =>
            r.ChatId == chatId
            && r.Text == text
            && r.ReplyToMessageId == null
            && r.DisableWebPagePreview == true
            && r.ParseMode == ParseMode.Markdown), It.IsAny<CancellationToken>()), Times.Once);
        _mockTelegramBotClient.VerifyNoOtherCalls();
    }
}