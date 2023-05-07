using MiroslavGPT.Domain.Actions;
using MiroslavGPT.Domain.Interfaces.Clients;

namespace MiroslavGPT.Domain.Tests.Actions;

[TestFixture]
public class ExceptionActionTests
{
    private Fixture _fixture = null!;
    private Mock<ITelegramClient> _mockTelegramClient = null!;
    private ExceptionAction _action = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _mockTelegramClient = _fixture.Freeze<Mock<ITelegramClient>>();
        _action = _fixture.Create<ExceptionAction>();
    }
    
    [Test, AutoData]
    public async Task ExecuteAsync_ShouldSendMessage(long chatId, int messageId)
    {
        // Arrange
        // Act
        await _action.ExecuteAsync(chatId, messageId);
        
        // Assert
        _mockTelegramClient.Verify(c => c.SendTextMessageAsync(chatId, "Something went wrong. Please try again later.", messageId));
    }
}