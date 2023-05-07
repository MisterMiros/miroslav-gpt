using MiroslavGPT.Domain.Actions;
using MiroslavGPT.Domain.Interfaces.Clients;
using MiroslavGPT.Domain.Models.Commands;
using Telegram.Bot.Types;

namespace MiroslavGPT.Domain.Tests.Actions;

[TestFixture]
public class UnknownActionTests
{
    private Fixture _fixture = null!;
    private Mock<ITelegramClient> _mockTelegramClient = null!;
    private UnknownAction _action = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new();
        _fixture.Customize(new AutoMoqCustomization());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _mockTelegramClient = _fixture.Freeze<Mock<ITelegramClient>>();
        _action = _fixture.Create<UnknownAction>();
    }

    [Test]
    public void TryGetCommand_ReturnsCommand()
    {
        // Arrange
        var update = _fixture.Create<Update>();
        
        // Act
        var result = _action.TryGetCommand(update);
        
        // Assert
        result.Should().NotBeNull();
        result.ChatId.Should().Be(update.Message!.Chat.Id);
        result.MessageId.Should().Be(update.Message.MessageId);
    }

    [Test, AutoData]
    public async Task ExecuteAsync_ShouldSendMessage(UnknownCommand command)
    {
        // Arrange
        // Act
        await _action.ExecuteAsync(command);
        
        // Assert
        _mockTelegramClient.Verify(c => c.SendTextMessageAsync(command.ChatId, "Unknown command. Please use /init or one of the personality commands.", command.MessageId));
    }
}