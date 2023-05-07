using MiroslavGPT.Domain.Actions;
using MiroslavGPT.Domain.Interfaces.Clients;
using MiroslavGPT.Domain.Models.Commands;
using MiroslavGPT.Domain.Settings;
using Telegram.Bot.Types;

namespace MiroslavGPT.Domain.Tests.Actions;

[TestFixture]
public class InitActionTests
{
    private Fixture _fixture = null!;
    private Mock<ITelegramClient> _mockTelegramClient = null!;
    private Mock<IChatGptBotSettings> _mockSettings = null!;
    private InitAction _action = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        _mockTelegramClient = _fixture.Freeze<Mock<ITelegramClient>>();
        _mockSettings = _fixture.Freeze<Mock<IChatGptBotSettings>>();
        _action = _fixture.Create<InitAction>();
    }

    [Test]
    public void TryGetCommand_ReturnsNull()
    {
        // Arrange
        var update = _fixture.Create<Update>();
        
        // Act
        var result = _action.TryGetCommand(update);
        
        // Assert
        result.Should().BeNull();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    [TestCase("some-secret-key")]
    [TestCase(" some-secret-key ")]
    public void TryGetCommand_ReturnsCommand(string? secretKey)
    {
        // Arrange
        var update = _fixture.Create<Update>();
        update.Message!.Text = $"/init {secretKey}";
        
        // Act
        var result = _action.TryGetCommand(update);
        
        // Assert
        result.Should().NotBeNull();
        var initCommand = result.Should().BeOfType<InitCommand>().Subject;
        initCommand.ChatId.Should().Be(update.Message.Chat.Id);
        initCommand.MessageId.Should().Be(update.Message.MessageId);
        initCommand.Secret.Should().Be((secretKey ?? string.Empty).Trim());
    }
    
    [Test, AutoData]
    public async Task ExecuteAsync_SendSuccessMessage_WhenAuthorized(InitCommand command)
    {
        // Arrange
        _mockSettings.Setup(s => s.SecretKey)
            .Returns(command.Secret!);
        
        // Act
        await _action.ExecuteAsync(command);
        
        // Assert
        _mockTelegramClient.Verify(c => c.SendTextMessageAsync(command.ChatId, "Authorization successful! You can now use prompt commands.", command.MessageId));
    }
    
    [Test, AutoData]
    public async Task ExecuteAsync_SendFailedMessage_WhenNotAuthorized(InitCommand command)
    {
        // Arrange
        _mockSettings.Setup(s => s.SecretKey)
            .Returns(_fixture.Create<string>());
        
        // Act
        await _action.ExecuteAsync(command);
        
        // Assert
        _mockTelegramClient.Verify(c => c.SendTextMessageAsync(command.ChatId, "Incorrect secret key. Please try again.", command.MessageId));
    }
}