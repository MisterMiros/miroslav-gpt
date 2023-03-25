using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Personalities;
using MiroslavGPT.Domain.Settings;

namespace MiroslavGPT.Domain.Tests
{
    [TestFixture]
    public class ChatGPTBotTests
    {
        private Fixture _fixture;
        private IMock<IUsersRepository> _mockUserRepository;
        private IMock<IPersonalityProvider> _mockPersonalityProvider;
        private IMock<IChatGptBotSettings> _mockSettings;
        private ChatGPTBot _chatGptBot;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());

            _mockUserRepository = _fixture.Freeze<IMock<IUsersRepository>>();
            _mockPersonalityProvider = _fixture.Freeze<IMock<IPersonalityProvider>>();
            _mockSettings = _fixture.Freeze<IMock<IChatGptBotSettings>>();
            _chatGptBot = _fixture.Create<ChatGPTBot>();
        }

        [Test]
        public async Task ProcessCommandAsync_ShouldReturnUnkownCommand_IfNotInitOrPrompt()
        {
            // Arrange
            var chatId = _fixture.Create<long>();
            var username = _fixture.Create<string>();
            var text = _fixture.Create<string>();

            // Act
            var result = await _chatGptBot.ProcessCommandAsync(chatId, username, text);

            // Assert
            result.Should().Be("Unknown command. Please use /init or /prompt.");
        }
    }
}
