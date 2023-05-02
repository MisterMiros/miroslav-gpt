using MiroslavGPT.Domain.Actions;
using MiroslavGPT.Domain.Extensions;
using MiroslavGPT.Domain.Interfaces.Clients;
using MiroslavGPT.Domain.Interfaces.Personality;
using MiroslavGPT.Domain.Interfaces.Threads;
using MiroslavGPT.Domain.Interfaces.Users;
using MiroslavGPT.Domain.Models;
using MiroslavGPT.Domain.Models.Commands;
using OpenAI_API.Chat;
using Telegram.Bot.Types;

namespace MiroslavGPT.Domain.Tests.Actions;

[TestFixture]
public class PromptActionTests
{
    private Fixture _fixture;
    private Mock<IUsersRepository> _mockUserRepository;
    private Mock<IThreadRepository> _mockThreadRepository;
    private Mock<IPersonalityProvider> _mockPersonalityProvider;
    private Mock<IChatClient> _mockChatClient;
    private Mock<ITelegramClient> _mockTelegramClient;
    private PromptAction _action;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        _mockUserRepository = _fixture.Freeze<Mock<IUsersRepository>>();
        _mockThreadRepository = _fixture.Freeze<Mock<IThreadRepository>>();
        _mockPersonalityProvider = _fixture.Freeze<Mock<IPersonalityProvider>>();
        _mockChatClient = _fixture.Freeze<Mock<IChatClient>>();
        _mockTelegramClient = _fixture.Freeze<Mock<ITelegramClient>>();
        _action = _fixture.Create<PromptAction>();
    }

    [Test]
    public void TryGetCommand_ReturnsNull_WhenNoPersonality()
    {
        // Arrange
        var update = _fixture.Create<Update>();

        _mockPersonalityProvider.Setup(p => p.HasPersonalityCommand(It.IsAny<string>()))
            .Returns(false);
        
        // Act
        var result = _action.TryGetCommand(update);
        
        // Assert
        result.Should().BeNull();
    }
    
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("some-prompt")]
    [TestCase(" some prompt with spaces ")]
    [TestCase(null)]
    public void TryGetCommand_ReturnsNull_WhenHasPersonality(string prompt)
    {
        // Arrange
        var update = _fixture.Create<Update>();
        update.Message!.Text = $"/prompt {prompt}";
        update.Message!.ReplyToMessage = _fixture.Create<Message>();

        _mockPersonalityProvider.Setup(p => p.HasPersonalityCommand("/prompt"))
            .Returns(true);
        
        // Act
        var result = _action.TryGetCommand(update);
        
        // Assert
        result.Should().NotBeNull();
        result.ChatId.Should().Be(update.Message.Chat.Id);
        result.MessageId.Should().Be(update.Message.MessageId);
        result.Personality.Should().Be("/prompt");
        result.Username.Should().Be(update.Message.From!.Username);
        result.Prompt.Should().Be((prompt ?? "").Trim());
        result.ReplyToId.Should().Be(update.Message.ReplyToMessage!.MessageId);
    }
    
    [Test]
    public async Task ExecuteAsync_SendsUnauthorizedMessage_WhenUserIsNotAuthorized()
    {
        // Arrange
        var command = _fixture.Create<PromptCommand>();
        _mockUserRepository.Setup(u => u.IsAuthorizedAsync(command.ChatId))
            .ReturnsAsync(false);
        
        // Act
        await _action.ExecuteAsync(command);
        
        // Assert
        _mockTelegramClient.Verify(t => t.SendTextMessageAsync(command.ChatId, "You are not authorized. Please use /init command with the correct secret key.", command.MessageId));
    }
    
    [TestCase("")]
    [TestCase("  ")]
    [TestCase(null)]
    public async Task ExecuteAsync_SendsProvideAPromptMessage_WhenEmptyPrompt(string prompt)
    {
        // Arrange
        var command = _fixture.Build<PromptCommand>()
            .With(c => c.Prompt, prompt)
            .Create();
        _mockUserRepository.Setup(u => u.IsAuthorizedAsync(command.ChatId))
            .ReturnsAsync(true);

        // Act
        await _action.ExecuteAsync(command);
        
        // Assert
        _mockTelegramClient.Verify(t => t.SendTextMessageAsync(command.ChatId, "Please provide a prompt after the personality command.", command.MessageId));
    }

    [TestCase(false, false)]
    [TestCase(true, true)]
    [TestCase(false, true)]
    public async Task ExecuteAsync_SendsResponse(bool threadExists, bool hasReplyTo)
    {
        // Arrange
        var command = _fixture.Create<PromptCommand>();
        var threadId = _fixture.Create<Guid>();
        var response = _fixture.Create<string>();
        var message = _fixture.Create<Message>();
        if (!hasReplyTo)
        {
            command.ReplyToId = null;
        }
        _mockUserRepository.Setup(u => u.IsAuthorizedAsync(command.ChatId))
            .ReturnsAsync(true);

        if (hasReplyTo)
        {
            _mockThreadRepository.Setup(r => r.GetThreadByMessageAsync(command.ChatId, command.ReplyToId!.Value))
                .ReturnsAsync(threadExists ? threadId : null);
        }
        _mockThreadRepository.Setup(r => r.CreateThreadAsync(command.ChatId))
            .ReturnsAsync(threadId);
        
        var personality = _fixture.CreateMany<ChatMessage>().ToList();
        _mockPersonalityProvider.Setup(p => p.GetPersonalityMessages(command.Personality))
            .Returns(personality);
        
        var threadMessages = new List<ThreadMessage>
        {
            _fixture.Build<ThreadMessage>().With(m => m.IsAssistant, false).Create(),
            _fixture.Build<ThreadMessage>().With(m => m.IsAssistant, true).Create(),
            _fixture.Build<ThreadMessage>()
                .With(m => m.MessageId, command.MessageId)
                .With(m => m.IsAssistant, false)
                .With(m => m.Username, command.Username)
                .With(m => m.Text, command.Prompt)
                .Create()
        };
        _mockThreadRepository.Setup(r => r.GetMessagesAsync(threadId))
            .ReturnsAsync(threadMessages);
        
        _mockTelegramClient.Setup(c => c.SendTextMessageAsync(command.ChatId, $"*Response from ChatGPT API for prompt '{command.Prompt}':*\n\n{response}", command.MessageId))
            .ReturnsAsync(message);

        var chatMessages = new List<ChatMessage>();
        _mockChatClient.Setup(c => c.GetChatGptResponseAsync(command.Prompt, It.IsAny<List<ChatMessage>>()))
            .Callback((string _, List<ChatMessage> m) =>
            {
                chatMessages = m;
            })
            .ReturnsAsync(response);
        
        // Act
        await _action.ExecuteAsync(command);
        
        // Assert
        if (hasReplyTo)
        {
            _mockThreadRepository.Verify(r => r.GetThreadByMessageAsync(command.ChatId, command.ReplyToId!.Value), Times.Once);
            if (threadExists)
            {
                _mockThreadRepository.Verify(r => r.CreateThreadAsync(It.IsAny<long>()), Times.Never);   
            }
            else
            {
                _mockThreadRepository.Verify(r => r.CreateThreadAsync(command.ChatId), Times.Once);
            }
        } else
        {
            _mockThreadRepository.Verify(r => r.CreateThreadAsync(command.ChatId), Times.Once);
        }
        
        _mockThreadRepository.Verify(r => r.GetMessagesAsync(threadId), Times.Once);
        _mockThreadRepository.Verify(r => r.AddThreadMessageAsync(threadId, command.MessageId, command.Prompt, command.Username), Times.Once);
        _mockThreadRepository.Verify(r => r.AddThreadMessageAsync(threadId, message.MessageId, response, null), Times.Once);
        _mockThreadRepository.VerifyNoOtherCalls();
        
        _mockTelegramClient.Verify(c => c.SendTextMessageAsync(command.ChatId, $"*Response from ChatGPT API for prompt '{command.Prompt}':*\n\n{response}", command.MessageId), Times.Once);
        _mockTelegramClient.VerifyNoOtherCalls();
        
        _mockChatClient.Verify(c => c.GetChatGptResponseAsync(command.Prompt, It.IsAny<List<ChatMessage>>()), Times.Once);
        _mockChatClient.VerifyNoOtherCalls();
        
        chatMessages.Should().NotBeEmpty();
        chatMessages.Take(personality.Count).Should().BeEquivalentTo(personality);
        chatMessages.Skip(personality.Count).Take(threadMessages.Count).Should().BeEquivalentTo(threadMessages.Select(m => m.ToChatMessage()));
        chatMessages.Should().HaveCount(personality.Count + threadMessages.Count);
    }
}