using Microsoft.Azure.Cosmos;
using MiroslavGPT.Azure.Settings;
using MiroslavGPT.Azure.Threads;
using MiroslavGPT.Domain.Models.Threads;

namespace MiroslavGPT.Azure.Tests.Threads;

[TestFixture]
public class CosmosThreadsRepositoryTests
{
    private Fixture _fixture = null!;
    private Mock<IThreadSettings> _mockSettings = null!;
    private Mock<CosmosClient> _mockCosmosClient = null!;
    private Mock<Container> _mockContainer = null!;
    private CosmosThreadRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new();
        _fixture.Customize(new AutoMoqCustomization());

        _mockContainer = _fixture.Freeze<Mock<Container>>();
        _mockCosmosClient = _fixture.Freeze<Mock<CosmosClient>>();
        _mockCosmosClient.Setup(c => c.GetContainer(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(_mockContainer.Object);
        _mockSettings = _fixture.Freeze<Mock<IThreadSettings>>();

        _repository = new(_mockCosmosClient.Object, _mockSettings.Object);
    }

    private static bool ThreadsEqual(CosmosThreadRepository.CosmosMessageThread cosmosThread, MessageThread thread)
    {
        return cosmosThread.Id == thread.Id.ToString() &&
               cosmosThread.ChatId == thread.ChatId &&
               cosmosThread.Messages.Count == thread.Messages.Count &&
               cosmosThread.Messages.Zip(thread.Messages).All(pair =>
                   pair.First.MessageId == pair.Second.MessageId &&
                   pair.First.Text == pair.Second.Text &&
                   pair.First.IsAssistant == pair.Second.IsAssistant &&
                   pair.First.Username == pair.Second.Username
               );
    }

    [Test, AutoData]
    public async Task CreateThreadAsync_ShouldCreate(long chatId)
    {
        // Arrange
        // Act
        var thread = await _repository.CreateThreadAsync(chatId);
        // Assert
        _mockContainer.Verify(c => c.CreateItemAsync(
            It.Is<CosmosThreadRepository.CosmosMessageThread>(t => ThreadsEqual(t, thread)),
            It.Is<PartitionKey>(k => k == new PartitionKey(thread.Id.ToString())),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()));
    }

    [Test, AutoData]
    public async Task GetThreadByMessage_ShouldReturnNull_WhenNotFoundException(long chatId, int messageId)
    {
        // Arrange
        _mockContainer.Setup(c => c.GetItemQueryIterator<CosmosThreadRepository.CosmosMessageThread>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Throws(new CosmosException("Not found", System.Net.HttpStatusCode.NotFound, 0, "", 0));

        // Act
        var result = await _repository.GetThreadByMessageAsync(chatId, messageId);
        // Assert
        result.Should().BeNull();
    }

    [Test, AutoData]
    public async Task GetThreadByMessage_ShouldReturnNull_WhenNoValuesInIterator(long chatId, int messageId)
    {
        // Arrange
        var iterator = _fixture.Create<Mock<FeedIterator<MessageThread>>>();
        iterator.Setup(i => i.HasMoreResults).Returns(false);

        _mockContainer.Setup(c => c.GetItemQueryIterator<MessageThread>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Returns(iterator.Object);

        // Act
        var result = await _repository.GetThreadByMessageAsync(chatId, messageId);
        // Assert
        result.Should().BeNull();
    }

    [Test, AutoData]
    public async Task GetThreadByMessage_ShouldReturnThread(long chatId, int messageId, CosmosThreadRepository.CosmosMessageThread cosmosThread)
    {
        // Arrange
        cosmosThread.Id = Guid.NewGuid().ToString();
        var threads = new List<CosmosThreadRepository.CosmosMessageThread> { cosmosThread };

        var feedResponse = _fixture.Create<Mock<FeedResponse<CosmosThreadRepository.CosmosMessageThread>>>();
        feedResponse.Setup(r => r.GetEnumerator())
            .Returns(threads.GetEnumerator());

        var iterator = _fixture.Create<Mock<FeedIterator<CosmosThreadRepository.CosmosMessageThread>>>();
        iterator.Setup(i => i.HasMoreResults).Returns(true);
        iterator.Setup(i => i.ReadNextAsync(default))
            .ReturnsAsync(feedResponse.Object);

        _mockContainer.Setup(c => c.GetItemQueryIterator<CosmosThreadRepository.CosmosMessageThread>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Returns(iterator.Object);

        // Act
        var result = await _repository.GetThreadByMessageAsync(chatId, messageId);

        // Assert
        result.Should().NotBeNull().And.Match(t => ThreadsEqual(cosmosThread, (MessageThread)t));
    }

    [Test, AutoData]
    public async Task UpdateThreadAsync_ShouldUpdate(MessageThread messageThread)
    {
        // Arrange
        _mockSettings.Setup(s => s.ThreadLengthLimit).Returns(1);
        
        // Act
        await _repository.UpdateThreadAsync(messageThread);

        // Assert
        _mockContainer.Verify(c => c.ReplaceItemAsync(
                It.Is<CosmosThreadRepository.CosmosMessageThread>(t => t.Id == messageThread.Id.ToString() &&
                                                                       t.ChatId == messageThread.ChatId &&
                                                                       t.Messages.Count == 1 &&
                                                                       t.Messages.Single().MessageId == messageThread.Messages.Last().MessageId &&
                                                                       t.Messages.Single().Text == messageThread.Messages.Last().Text &&
                                                                       t.Messages.Single().IsAssistant == messageThread.Messages.Last().IsAssistant &&
                                                                       t.Messages.Single().Username == messageThread.Messages.Last().Username),
                messageThread.Id.ToString(),
                It.Is<PartitionKey>(k => k == new PartitionKey(messageThread.Id.ToString())),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once()
        );
        _mockContainer.VerifyNoOtherCalls();
    }
}