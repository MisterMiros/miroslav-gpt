using MiroslavGPT.Domain.Clients;
using MiroslavGPT.Domain.Settings;
using OpenAI_API.Chat;

namespace MiroslavGPT.Domain.Tests.Clients;

[TestFixture]
public class ChatClientTests
{
    private Fixture _fixture = null!;
    private Mock<IChatEndpoint> _mockChatEndpoint = null!;
    private Mock<IChatGptBotSettings> _mockSettings = null!;
    private ChatClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new();
        _fixture.Customize(new AutoMoqCustomization());

        _mockChatEndpoint = _fixture.Freeze<Mock<IChatEndpoint>>();
        _mockSettings = _fixture.Freeze<Mock<IChatGptBotSettings>>();
        _client = _fixture.Create<ChatClient>();
    }

    [Test, AutoData]
    public async Task GetChatGptResponseAsync_ShouldSendRequest(string prompt, List<ChatMessage> messages, ChatResult chatResult, int maxTokens)
    {
        // Arrange
        var expectedResult = string.Join("\n", chatResult.Choices.Select(c => c.Message.Content.Trim()));
        _mockChatEndpoint.Setup(e => e.CreateChatCompletionAsync(It.IsAny<ChatRequest>()))
            .ReturnsAsync(chatResult);
        _mockSettings.Setup(s => s.MaxTokens).Returns(maxTokens);
        // Act
        var result = await _client.GetChatGptResponseAsync(prompt, messages);

        // Assert
        result.Should().Be(expectedResult);
        _mockChatEndpoint.Verify(e => e.CreateChatCompletionAsync(It.Is<ChatRequest>(r =>
            r.MaxTokens == maxTokens &&
            Equals(r.Messages, messages) &&
            r.Model == OpenAI_API.Models.Model.ChatGPTTurbo.ModelID &&
            r.Temperature == 0.7 &&
            r.TopP == 1 &&
            r.FrequencyPenalty == 0 &&
            r.PresencePenalty == 0
        )));
    }
}