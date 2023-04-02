using MiroslavGPT.Domain.Factories;
using Telegram.Bot;

namespace MiroslavGPT.Domain.Tests
{
    public class TelegramClientFactoryTests
    {
        private TelegramClientFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _factory = new TelegramClientFactory();
        }

        [Test]
        public void CreateTelegramClient_ShouldWork()
        {
            // Arrange
            var token = "123456:ABC-DEF1234ghIkl-zyx57W2v1u123ew11";
            // Act
            var client = _factory.CreateBotClient(token);

            // Assert
            client.Should().NotBeNull().And.BeOfType<TelegramBotClient>().Which.BotId.Should().Be(123456);
        }

    }
}
