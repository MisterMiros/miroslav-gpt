using MiroslavGPT.Domain.Interfaces.Actions;
using MiroslavGPT.Domain.Models.Commands;
using MiroslavGPT.Domain.Settings;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MiroslavGPT.Domain.Tests;

[TestFixture]
public class TelegramMessageHandlerTests
{
    private Fixture _fixture;
    private List<Mock<IAction<ICommand>>> _mockActions;
    private Mock<IExceptionAction> _mockExceptionAction;
    private Mock<ITelegramBotSettings> _mockSettings;
    private TelegramMessageHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _mockActions = new List<Mock<IAction<ICommand>>>
        {
            new(),
            new(),
            new(),
        };
        _mockExceptionAction = _fixture.Freeze<Mock<IExceptionAction>>();
        _mockSettings = _fixture.Freeze<Mock<ITelegramBotSettings>>();

        _handler = new TelegramMessageHandler(
            _mockActions.Select(a => a.Object),
            _mockExceptionAction.Object,
            _mockSettings.Object);
    }

    [Test]
    public async Task ProcessUpdateAsync_ShouldSkip_WhenUpdateEmpty()
    {
        // Arrange
        // Act
        await _handler.ProcessUpdateAsync(null);

        // Assert
        foreach (var action in _mockActions)
        {
            action.VerifyNoOtherCalls();
        }
        _mockExceptionAction.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ProcessUpdateAsync_ShouldSkip_WhenUpdateMessageEmpty()
    {
        // Arrange
        var update = _fixture.Build<Update>().With(r => r.Message, (Message?)null).Create();

        // Act
        await _handler.ProcessUpdateAsync(update);

        // Assert
        foreach (var action in _mockActions)
        {
            action.VerifyNoOtherCalls();
        }
        _mockExceptionAction.VerifyNoOtherCalls();
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
        foreach (var action in _mockActions)
        {
            action.VerifyNoOtherCalls();
        }
        _mockExceptionAction.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ProcessUpdateAsync_ShouldSkip_WhenNotACommand()
    {
        // Arrange
        var update = _fixture.Create<Update>();

        // Act
        await _handler.ProcessUpdateAsync(update);

        // Assert
        foreach (var action in _mockActions)
        {
            action.VerifyNoOtherCalls();
        }
        _mockExceptionAction.VerifyNoOtherCalls();
    }

    [TestCase(ChatType.Supergroup)]
    [TestCase(ChatType.Channel)]
    [TestCase(ChatType.Sender)]
    public async Task ProcessUpdateAsync_ShouldSkip_WhenNotSupportedGroup(ChatType chatType)
    {
        // Arrange
        var botName = "thebot";
        var update = _fixture.Create<Update>();
        update.Message!.Text = "/command";
        update.Message.Chat.Type = chatType;

        _mockSettings.Setup(s => s.TelegramBotUsername)
            .Returns(botName);

        // Act
        await _handler.ProcessUpdateAsync(null);

        // Assert
        foreach (var action in _mockActions)
        {
            action.VerifyNoOtherCalls();
        }
        _mockExceptionAction.VerifyNoOtherCalls();
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
        var botName = "botname";
        var update = _fixture.Create<Update>();
        update.Message!.Text = text;
        update.Message.Chat.Type = ChatType.Group;

        _mockSettings.Setup(s => s.TelegramBotUsername)
            .Returns(botName);

        // Act
        await _handler.ProcessUpdateAsync(null);

        // Assert
        foreach (var action in _mockActions)
        {
            action.VerifyNoOtherCalls();
        }
        _mockExceptionAction.VerifyNoOtherCalls();
    }

    [TestCase(0, ChatType.Private)]
    [TestCase(1, ChatType.Private)]
    [TestCase(2, ChatType.Private)]
    [TestCase(0, ChatType.Group)]
    [TestCase(1, ChatType.Group)]
    [TestCase(2, ChatType.Group)]
    public async Task ProcessUpdateAsync_ShouldExecuteFirstAction_ThatReturnsCommand(int actionNumber, ChatType chatType)
    {
        // Arrange
        var text = chatType == ChatType.Private ? "/command" : "/command@botname";
        var update = _fixture.Create<Update>();
        update.Message!.Text = text;
        update.Message.Chat.Type = chatType;
            
        _mockSettings.Setup(s => s.TelegramBotUsername)
            .Returns("botname");

        var mockCommand = _fixture.Create<Mock<ICommand>>();
        _mockActions[actionNumber].Setup(a => a.TryGetCommand(update))
            .Returns(mockCommand.Object);

        // Act
        await _handler.ProcessUpdateAsync(update);
            
        // Assert
        foreach (var action in _mockActions.Take(actionNumber))
        {
            action.Verify(a => a.TryGetCommand(update), Times.Once);
            action.VerifyNoOtherCalls();
        }
            
        _mockActions[actionNumber].Verify(a => a.TryGetCommand(update), Times.Once);
        _mockActions[actionNumber].Verify(a => a.ExecuteAsync(mockCommand.Object), Times.Once);

        foreach (var action in _mockActions.Skip(actionNumber + 1))
        {
            action.VerifyNoOtherCalls();
        }
    }
        
    [TestCase(ChatType.Private)]
    [TestCase(ChatType.Group)]
    public void ProcessUpdateAsync_ShouldExecuteExceptionAction_WhenException(ChatType chatType)
    {
        // Arrange
        var text = chatType == ChatType.Private ? "/command" : "/command@botname";
        var update = _fixture.Create<Update>();
        update.Message!.Text = text;
        update.Message.Chat.Type = chatType;
            
        _mockSettings.Setup(s => s.TelegramBotUsername)
            .Returns("botname");

        var exception = new Exception("I failed");
        _mockActions[0].Setup(a => a.TryGetCommand(update))
            .Throws(exception);

        // Act
        Assert.ThrowsAsync<Exception>(async () =>  await _handler.ProcessUpdateAsync(update))
            .Should().Be(exception);
            
        // Assert
        _mockActions[0].Verify(a => a.TryGetCommand(update), Times.Once);
        foreach (var action in _mockActions)
        {
            action.VerifyNoOtherCalls();
        }
        _mockExceptionAction.Verify(a => a.ExecuteAsync(update.Message.Chat.Id, update.Message.MessageId));
        _mockExceptionAction.VerifyNoOtherCalls();
    }
}