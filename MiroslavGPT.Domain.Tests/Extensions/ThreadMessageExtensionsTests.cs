using MiroslavGPT.Domain.Extensions;
using MiroslavGPT.Domain.Models;
using OpenAI_API.Chat;

namespace MiroslavGPT.Domain.Tests.Extensions;

[TestFixture]
public class ThreadMessageExtensionsTests
{
    private readonly Fixture _fixture = new();

    [TestCase(true, null)]
    [TestCase(true, "")]
    [TestCase(true, "  ")]
    [TestCase(true, "username")]
    [TestCase(false, null)]
    [TestCase(false, "")]
    [TestCase(false, "  ")]
    [TestCase(false, "username")]
    public void ToChatMessage_Works(bool isAssistant, string username)
    {
        // Arrange
        var threadMessage = _fixture.Build<ThreadMessage>()
            .With(m => m.IsAssistant, isAssistant)
            .With(m => m.Username, username)
            .Create();

        // Act
        var result = threadMessage.ToChatMessage();

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(isAssistant ? ChatMessageRole.Assistant : ChatMessageRole.User);
        result.Content.Should().Be(string.IsNullOrWhiteSpace(username) ? threadMessage.Text : $"@{username}: {threadMessage.Text}");
    } 
}