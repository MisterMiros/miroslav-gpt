using MiroslavGPT.Domain.Factories;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Personalities;
using MiroslavGPT.Domain.Settings;
using OpenAI_API.Chat;

namespace MiroslavGPT.Domain.Tests
{
    [TestFixture]
    public class ChatGPTBotTests
    {
        private Fixture _fixture;
        private Mock<IUsersRepository> _mockUserRepository;
        private Mock<IPersonalityProvider> _mockPersonalityProvider;
        private Mock<IChatGptBotSettings> _mockSettings;
        private Mock<IOpenAiClientFactory> _mockOpenAiClientFactory;
        private Mock<IChatEndpoint> _mockChatClient;
        private ChatGPTBot _chatGptBot;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());

            _mockUserRepository = _fixture.Freeze<Mock<IUsersRepository>>();
            _mockPersonalityProvider = _fixture.Freeze<Mock<IPersonalityProvider>>();
            _mockSettings = _fixture.Freeze<Mock<IChatGptBotSettings>>();
            _mockOpenAiClientFactory = _fixture.Freeze<Mock<IOpenAiClientFactory>>();
            _mockChatClient = _fixture.Create<Mock<IChatEndpoint>>();
            _mockOpenAiClientFactory.Setup(f => f.CreateChatClient(It.IsAny<string>()))
                .Returns(_mockChatClient.Object);
            _chatGptBot = _fixture.Create<ChatGPTBot>();
        }

        [Test]
        public async Task ProcessCommandAsync_ShouldReturnUnkownCommand()
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

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public async Task ProcessCommandAsync_ShouldReturnNull_WhenEmptyText(string text)
        {
            // Arrange
            var chatId = _fixture.Create<long>();
            var username = _fixture.Create<string>();

            // Act
            var result = await _chatGptBot.ProcessCommandAsync(chatId, username, text);

            // Assert
            result.Should().Be(null);
        }

        [TestCase("no_spaces_key")]
        [TestCase("spaces key")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque ac ultrices diam. Ut blandit arcu a nisi tristique, at lobortis metus facilisis. Sed efficitur ante nec felis dignissim viverra. Praesent dignissim nulla quis nunc placerat interdum. Duis non eros vel leo bibendum cursus non cursus augue. Aliquam porta tempus convallis. Donec quis placerat nisi. Suspendisse quis porttitor nunc. Integer ullamcorper lobortis lectus. Etiam rutrum magna felis, et commodo dolor sollicitudin fringilla. Sed mollis tempor justo, in accumsan metus blandit a. Phasellus placerat et lacus non faucibus. Integer et sem nec erat tincidunt vestibulum. Fusce vitae orci porttitor, semper odio vitae, pulvinar velit. Aliquam felis mi, faucibus non quam non, blandit suscipit ipsum. Sed non cursus magna, et vestibulum purus.")]
        [TestCase("")]
        [TestCase("  ")]
        public async Task ProcessCommandAsync_Init_ShouldNotAuthorize_IfWrongSecretKey(string wrongKey)
        {
            // Arrange
            var chatId = _fixture.Create<long>();
            var username = _fixture.Create<string>();
            var text = $"/init {wrongKey}";

            _mockSettings.Setup(s => s.SecretKey).Returns(_fixture.Create<string>());

            // Act
            var result = await _chatGptBot.ProcessCommandAsync(chatId, username, text);

            // Assert
            result.Should().Be("Incorrect secret key. Please try again.");
            _mockUserRepository.Verify(r => r.AuthorizeUserAsync(It.IsAny<long>()), Times.Never);
        }

        [TestCase("no_spaces_key")]
        [TestCase("  spaces key  ")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque ac ultrices diam. Ut blandit arcu a nisi tristique, at lobortis metus facilisis. Sed efficitur ante nec felis dignissim viverra. Praesent dignissim nulla quis nunc placerat interdum. Duis non eros vel leo bibendum cursus non cursus augue. Aliquam porta tempus convallis. Donec quis placerat nisi. Suspendisse quis porttitor nunc. Integer ullamcorper lobortis lectus. Etiam rutrum magna felis, et commodo dolor sollicitudin fringilla. Sed mollis tempor justo, in accumsan metus blandit a. Phasellus placerat et lacus non faucibus. Integer et sem nec erat tincidunt vestibulum. Fusce vitae orci porttitor, semper odio vitae, pulvinar velit. Aliquam felis mi, faucibus non quam non, blandit suscipit ipsum. Sed non cursus magna, et vestibulum purus.")]
        [TestCase("")]
        [TestCase("  ")]
        public async Task ProcessCommandAsync_Init_ShouldAuthorize_IfCorrectSecretKey(string secretKey)
        {
            // Arrange
            var chatId = _fixture.Create<long>();
            var username = _fixture.Create<string>();
            var text = $"/init {secretKey}";

            _mockSettings.Setup(s => s.SecretKey).Returns(secretKey.Trim());

            // Act
            var result = await _chatGptBot.ProcessCommandAsync(chatId, username, text);

            // Assert
            result.Should().Be("Authorization successful! You can now use /prompt command.");
            _mockUserRepository.Verify(r => r.AuthorizeUserAsync(It.IsAny<long>()), Times.Once);
            _mockUserRepository.Verify(r => r.AuthorizeUserAsync(chatId), Times.Once);
        }

        [Test]
        public async Task ProcessCommandAsync_Prompt_ShouldForbid_WhenNotAuthorized()
        {
            // Arrange
            var chatId = _fixture.Create<long>();
            var username = _fixture.Create<string>();
            var text = $"/prompt {_fixture.Create<string>()}";

            _mockUserRepository.Setup(r => r.IsAuthorizedAsync(chatId))
                .ReturnsAsync(false);

            // Act
            var result = await _chatGptBot.ProcessCommandAsync(chatId, username, text);


            // Assert
            result.Should().Be("You are not authorized. Please use /init command with the correct secret key.");
            _mockChatClient.Verify(c => c.CreateChatCompletionAsync(It.IsAny<ChatRequest>()), Times.Never);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("    ")]
        public async Task ProcessCommandAsync_Prompt_ShouldForbid_WhenNoPrompt(string prompt)
        {
            // Arrange
            var chatId = _fixture.Create<long>();
            var username = _fixture.Create<string>();
            var text = $"/prompt{prompt}";

            _mockUserRepository.Setup(r => r.IsAuthorizedAsync(chatId))
                .ReturnsAsync(true);

            // Act
            var result = await _chatGptBot.ProcessCommandAsync(chatId, username, text);


            // Assert
            result.Should().Be("Please provide a prompt after the /prompt command.");
            _mockChatClient.Verify(c => c.CreateChatCompletionAsync(It.IsAny<ChatRequest>()), Times.Never);
        }

        [TestCase(" ")]
        [TestCase("    ")]
        public async Task ProcessCommandAsync_Prompt_ShouldSendRequest_WhenCorrectPrompt(string spaces)
        {
            // Arrange
            var chatId = _fixture.Create<long>();
            var username = _fixture.Create<string>();
            var prompt = _fixture.Create<string>();
            var text = $"/prompt{spaces}{prompt}";
            var maxTokens = _fixture.Create<int>();

            _mockSettings.Setup(r => r.MaxTokens)
                .Returns(maxTokens);

            _mockUserRepository.Setup(r => r.IsAuthorizedAsync(chatId))
                .ReturnsAsync(true);

            var personality = _fixture.CreateMany<ChatMessage>().ToList();
            _mockPersonalityProvider.Setup(p => p.GetPersonalityMessages())
                .Returns(personality);

            ChatRequest sentRequest = null;
            var chatResult = _fixture.Create<ChatResult>();
            var response = string.Join("\n", chatResult.Choices.Select(c => c.Message.Content.Trim()));
            _mockChatClient.Setup(c => c.CreateChatCompletionAsync(It.IsAny<ChatRequest>()))
                .Callback((ChatRequest r) =>
                {
                    sentRequest = r;
                })
                .ReturnsAsync(chatResult);

            // Act
            var result = await _chatGptBot.ProcessCommandAsync(chatId, username, text);

            // Assert
            result.Should().Be($"*Response from ChatGPT API for prompt '{prompt}':*\n\n{response}");
            sentRequest.Should().NotBeNull();
            sentRequest.Messages.Should().NotBeEmpty();
            sentRequest.Messages.Take(sentRequest.Messages.Count - 1).Should().BeEquivalentTo(personality);
            sentRequest.Messages.Last().Should().BeEquivalentTo(new ChatMessage()
            {
                Role = ChatMessageRole.User,
                Content = $"@{username}: {prompt}"
            });
            sentRequest.MaxTokens.Should().Be(maxTokens);
            sentRequest.Model.Should().Be(OpenAI_API.Models.Model.ChatGPTTurbo.ModelID);
            sentRequest.Temperature.Should().Be(0.7);
            sentRequest.TopP.Should().Be(1);
            sentRequest.FrequencyPenalty.Should().Be(0);
            sentRequest.PresencePenalty.Should().Be(0);
            _mockChatClient.Verify(c => c.CreateChatCompletionAsync(It.IsAny<ChatRequest>()), Times.Once);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public async Task ProcessCommandAsync_Prompt_ShouldSendRequest_WhenNoUsername(string username)
        {
            // Arrange
            var chatId = _fixture.Create<long>();
            var prompt = _fixture.Create<string>();
            var text = $"/prompt {prompt}";
            var maxTokens = _fixture.Create<int>();

            _mockSettings.Setup(r => r.MaxTokens)
                .Returns(maxTokens);

            _mockUserRepository.Setup(r => r.IsAuthorizedAsync(chatId))
                .ReturnsAsync(true);

            var personality = _fixture.CreateMany<ChatMessage>().ToList();
            _mockPersonalityProvider.Setup(p => p.GetPersonalityMessages())
                .Returns(personality);

            ChatRequest sentRequest = null;
            var chatResult = _fixture.Create<ChatResult>();
            var response = string.Join("\n", chatResult.Choices.Select(c => c.Message.Content.Trim()));
            _mockChatClient.Setup(c => c.CreateChatCompletionAsync(It.IsAny<ChatRequest>()))
                .Callback((ChatRequest r) =>
                {
                    sentRequest = r;
                })
                .ReturnsAsync(chatResult);

            // Act
            var result = await _chatGptBot.ProcessCommandAsync(chatId, username, text);

            // Assert
            result.Should().Be($"*Response from ChatGPT API for prompt '{prompt}':*\n\n{response}");
            sentRequest.Should().NotBeNull();
            sentRequest.Messages.Should().NotBeEmpty();
            sentRequest.Messages.Take(sentRequest.Messages.Count - 1).Should().BeEquivalentTo(personality);
            sentRequest.Messages.Last().Should().BeEquivalentTo(new ChatMessage()
            {
                Role = ChatMessageRole.User,
                Content = prompt,
            });
            sentRequest.MaxTokens.Should().Be(maxTokens);
            sentRequest.Model.Should().Be(OpenAI_API.Models.Model.ChatGPTTurbo.ModelID);
            sentRequest.Temperature.Should().Be(0.7);
            sentRequest.TopP.Should().Be(1);
            sentRequest.FrequencyPenalty.Should().Be(0);
            sentRequest.PresencePenalty.Should().Be(0);
            _mockChatClient.Verify(c => c.CreateChatCompletionAsync(It.IsAny<ChatRequest>()), Times.Once);
        }

        [Test]
        public async Task ProcessCommandAsync_Prompt_ShouldReturnError_WhenFailedApiRequest()
        {
            // Arrange
            var chatId = _fixture.Create<long>();
            var username = _fixture.Create<string>();
            var prompt = _fixture.Create<string>();
            var text = $"/prompt {prompt}";
            var maxTokens = _fixture.Create<int>();

            _mockSettings.Setup(r => r.MaxTokens)
                .Returns(maxTokens);

            _mockUserRepository.Setup(r => r.IsAuthorizedAsync(chatId))
                .ReturnsAsync(true);

            var personality = _fixture.CreateMany<ChatMessage>().ToList();
            _mockPersonalityProvider.Setup(p => p.GetPersonalityMessages())
                .Returns(personality);

            ChatRequest sentRequest = null;
            _mockChatClient.Setup(c => c.CreateChatCompletionAsync(It.IsAny<ChatRequest>()))
                .Callback((ChatRequest r) =>
                {
                    sentRequest = r;
                })
                .ThrowsAsync(new Exception("I failed"));

            // Act
            var result = await _chatGptBot.ProcessCommandAsync(chatId, username, text);

            // Assert
            result.Should().Be($"Error while fetching response from ChatGPT API: I failed");
            sentRequest.Should().NotBeNull();
            sentRequest.Messages.Should().NotBeEmpty();
            sentRequest.Messages.Take(sentRequest.Messages.Count - 1).Should().BeEquivalentTo(personality);
            sentRequest.Messages.Last().Should().BeEquivalentTo(new ChatMessage()
            {
                Role = ChatMessageRole.User,
                Content = $"@{username}: {prompt}"
            });
            sentRequest.MaxTokens.Should().Be(maxTokens);
            sentRequest.Model.Should().Be(OpenAI_API.Models.Model.ChatGPTTurbo.ModelID);
            sentRequest.Temperature.Should().Be(0.7);
            sentRequest.TopP.Should().Be(1);
            sentRequest.FrequencyPenalty.Should().Be(0);
            sentRequest.PresencePenalty.Should().Be(0);
            _mockChatClient.Verify(c => c.CreateChatCompletionAsync(It.IsAny<ChatRequest>()), Times.Once);
        }
    }
}
