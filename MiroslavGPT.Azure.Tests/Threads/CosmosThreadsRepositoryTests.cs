using Microsoft.Azure.Cosmos;
using MiroslavGPT.Azure.Settings;
using MiroslavGPT.Azure.Threads;
using Thread = MiroslavGPT.Domain.Models.Threads.Thread;

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
        var thread = await _repository.CreateThreadAsync(chatId);
        // Assert
        _mockContainer.Verify(c => c.CreateItemAsync(
            thread,
            It.Is<PartitionKey>(k => k == new PartitionKey(thread.Id.ToString())),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()));
    }

    [Test, AutoData]
    public async Task GetThreadByMessage_ShouldReturnNull_WhenNotFoundException(long chatId, int messageId)
    {
        // Arrange
        _mockContainer.Setup(c => c.GetItemQueryIterator<Thread>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
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
        var iterator = _fixture.Create<Mock<FeedIterator<Thread>>>();
        iterator.Setup(i => i.HasMoreResults).Returns(false);

        _mockContainer.Setup(c => c.GetItemQueryIterator<Thread>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Returns(iterator.Object);

        // Act
        var result = await _repository.GetThreadByMessageAsync(chatId, messageId);
        // Assert
        result.Should().BeNull();
    }

    [Test, AutoData]
    public async Task GetThreadByMessage_ShouldReturnThread(long chatId, int messageId, Thread thread)
    {
        // Arrange
        var threads = new List<Thread> { thread };

        var feedResponse = _fixture.Create<Mock<FeedResponse<Thread>>>();
        feedResponse.Setup(r => r.GetEnumerator())
            .Returns(threads.GetEnumerator());

        var iterator = _fixture.Create<Mock<FeedIterator<Thread>>>();
        iterator.Setup(i => i.HasMoreResults).Returns(true);
        iterator.Setup(i => i.ReadNextAsync(default))
            .ReturnsAsync(feedResponse.Object);

        _mockContainer.Setup(c => c.GetItemQueryIterator<Thread>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Returns(iterator.Object);

        // Act
        var result = await _repository.GetThreadByMessageAsync(chatId, messageId);

        // Assert
        result.Should().Be(thread);
    }

    [Test, AutoData]
    public async Task UpdateThreadAsync_ShouldUpdate(Thread thread)
    {
        // Arrange
        
        // Act
        await _repository.UpdateThreadAsync(thread);

        // Assert
        _mockContainer.Verify(c => c.ReplaceItemAsync(
                thread,
                thread.Id.ToString(),
                It.Is<PartitionKey>(k => k == new PartitionKey(thread.Id.ToString())),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once()
        );
        _mockContainer.VerifyNoOtherCalls();
    }
}