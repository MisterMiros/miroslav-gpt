using MiroslavGPT.Domain.Extensions;

namespace MiroslavGPT.Domain.Tests.Extensions;

[TestFixture]
public class StringExtensionsTests
{
    [Test]
    public void EscapeUsernameInMarkdown_ShouldEscapeMarkdownCharacters()
    {
        // Arrange
        var markdown = "Hello @World! My name is @miroslav_gpt and I'm a bot.";
        var usernames = new[] { "@World", "@miroslav_gpt" };

        // Act
        var result = markdown.EscapeUsernames();

        // Assert
        result.Should().Be("Hello @World! My name is @miroslav\\_gpt and I'm a bot.");
    }
}