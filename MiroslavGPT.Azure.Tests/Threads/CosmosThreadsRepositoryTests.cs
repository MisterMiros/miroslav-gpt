using Microsoft.Azure.Cosmos;
using MiroslavGPT.Azure.Settings;
using MiroslavGPT.Azure.Threads;
using MiroslavGPT.Azure.Users;

namespace MiroslavGPT.Azure.Tests.Threads;

[TestFixture]
public class CosmosThreadsRepositoryTests
{
    private Fixture _fixture;
    private Mock<ICosmosThreadSettings> _mockSettings;
    private Mock<CosmosClient> _mockCosmosClient;
    private Mock<Container> _mockContainer;
    private CosmosThreadRepository _repository;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _mockContainer = _fixture.Freeze<Mock<Container>>();
        _mockCosmosClient = _fixture.Freeze<Mock<CosmosClient>>();
        _mockCosmosClient.Setup(c => c.GetContainer(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(_mockContainer.Object);
        _mockSettings = _fixture.Freeze<Mock<ICosmosThreadSettings>>();

        _repository = new CosmosThreadRepository(_mockCosmosClient.Object, _mockSettings.Object);
    }

    [Test, AutoData]
    public async Task CreateThreadAsync_ShouldCreate(long chatId)
    {
        // Arrange
        // Act
        var id = await _repository.CreateThreadAsync(chatId);
        // Assert
        _mockContainer.Verify(c => c.CreateItemAsync(
            It.Is<CosmosThreadRepository.CosmosThread>(t => t.ChatId == chatId && t.Id == id && t.Messages != null && t.Messages.Count == 0),
            It.Is<PartitionKey>(k => k == new PartitionKey(id.ToString())),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()));
    }

    [Test, AutoData]
    public async Task GetThreadByMessage_ShouldReturnNull_WhenNotFoundException(long chatId, int messageId)
    {
        // Arrange
        _mockContainer.Setup(c => c.GetItemQueryIterator<CosmosThreadRepository.CosmosThread>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
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
        var iterator = _fixture.Create<Mock<FeedIterator<CosmosUserRepository.User>>>();
        iterator.Setup(i => i.HasMoreResults).Returns(false);

        _mockContainer.Setup(c => c.GetItemQueryIterator<CosmosUserRepository.User>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Returns(iterator.Object);

        // Act
        var result = await _repository.GetThreadByMessageAsync(chatId, messageId);
        // Assert
        result.Should().BeNull();
    }

    [Test, AutoData]
    public async Task GetThreadByMessage_ShouldReturnId(long chatId, int messageId, CosmosThreadRepository.CosmosThread thread)
    {
        // Arrange
        var threads = new List<CosmosThreadRepository.CosmosThread> { thread };

        var feedResponse = _fixture.Create<Mock<FeedResponse<CosmosThreadRepository.CosmosThread>>>();
        feedResponse.Setup(r => r.GetEnumerator())
            .Returns(threads.GetEnumerator());

        var iterator = _fixture.Create<Mock<FeedIterator<CosmosThreadRepository.CosmosThread>>>();
        iterator.Setup(i => i.HasMoreResults).Returns(true);
        iterator.Setup(i => i.ReadNextAsync(default))
            .ReturnsAsync(feedResponse.Object);

        _mockContainer.Setup(c => c.GetItemQueryIterator<CosmosThreadRepository.CosmosThread>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Returns(iterator.Object);

        // Act
        var result = await _repository.GetThreadByMessageAsync(chatId, messageId);

        // Assert
        result.Should().Be(thread.Id);
    }

    [Test, AutoData]
    public async Task AddThreadMessageAsync_ShouldAdd(Guid id, long messageId, string text, string username, bool isAssistant, CosmosThreadRepository.CosmosThread thread)
    {
        // Arrange
        thread.Id = id;
        var mockItemResponse = new Mock<ItemResponse<CosmosThreadRepository.CosmosThread>>();
        mockItemResponse.Setup(r => r.Resource).Returns(thread);
        _mockContainer.Setup(c => c.ReadItemAsync<CosmosThreadRepository.CosmosThread>(id.ToString(), new PartitionKey(id.ToString()), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockItemResponse.Object);
        // Act
        await _repository.AddThreadMessageAsync(id, messageId, text, username, isAssistant);

        // Assert
        _mockContainer.Verify(c => c.ReadItemAsync<CosmosThreadRepository.CosmosThread>(
                id.ToString(),
                It.Is<PartitionKey>(k => k == new PartitionKey(id.ToString())),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once());
        _mockContainer.Verify(c => c.ReplaceItemAsync(
                thread,
                id.ToString(),
                It.Is<PartitionKey>(k => k == new PartitionKey(id.ToString())),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once()
        );
        _mockContainer.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task GetMessagesAsync_ReturnsMessages(CosmosThreadRepository.CosmosThread thread)
    {
        // Arrange
        var mockItemResponse = new Mock<ItemResponse<CosmosThreadRepository.CosmosThread>>();
        mockItemResponse.Setup(r => r.Resource).Returns(thread);
        _mockContainer.Setup(c => c.ReadItemAsync<CosmosThreadRepository.CosmosThread>(thread.Id.ToString(), new PartitionKey(thread.Id.ToString()), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockItemResponse.Object);
        
        // Act
        var messages = await _repository.GetMessagesAsync(thread.Id);
        
        // Assert
        messages.Should().BeEquivalentTo(thread.Messages);
    }
}