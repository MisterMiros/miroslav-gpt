using MiroslavGPT.Domain.Factories;
using OpenAI_API.Chat;

namespace MiroslavGPT.Domain.Tests
{
    public class OpenAiClientFactoryTests
    {
        private OpenAiClientFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _factory = new OpenAiClientFactory();
        }

        [Test, AutoData]
        public void CreateChatClient_ShouldWork(string key)
        {
            // Arrange
            // Act
            var client = _factory.CreateChatClient(key);

            // Assert
            client.Should().NotBeNull().And.BeOfType<ChatEndpoint>();
        }
    }
}
