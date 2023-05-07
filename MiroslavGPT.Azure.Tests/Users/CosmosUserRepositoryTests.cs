using Microsoft.Azure.Cosmos;
using MiroslavGPT.Azure.Settings;
using MiroslavGPT.Azure.Users;

namespace MiroslavGPT.Azure.Tests.Users;

public class CosmosUserRepositoryTests
{
    private Fixture _fixture = null!;
    private Mock<IUserSettings> _mockSettings = null!;
    private Mock<CosmosClient> _mockCosmosClient = null!;
    private Mock<Container> _mockContainer = null!;
    private CosmosUserRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new();
        _fixture.Customize(new AutoMoqCustomization());

        _mockContainer = _fixture.Freeze<Mock<Container>>();
        _mockCosmosClient = _fixture.Freeze<Mock<CosmosClient>>();
        _mockCosmosClient.Setup(c => c.GetContainer(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(_mockContainer.Object);
        _mockSettings = _fixture.Freeze<Mock<IUserSettings>>();

        _repository = new(_mockCosmosClient.Object, _mockSettings.Object);
    }

    [Test]
    public async Task IsAuthorizedAsync_ShouldReturnFalse_WhenNotFoundException()
    {
        // Arrange
        var userId = _fixture.Create<long>();
        _mockContainer.Setup(c => c.GetItemQueryIterator<CosmosUserRepository.CosmosUser>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Throws(new CosmosException("Not found", System.Net.HttpStatusCode.NotFound, 0, "", 0));

        // Act
        var result = await _repository.IsAuthorizedAsync(userId);
        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task IsAuthorizedAsync_ShouldReturnFalse_WhenNoValuesInIterator()
    {
        // Arrange
        var userId = _fixture.Create<long>();
        var iterator = _fixture.Create<Mock<FeedIterator<CosmosUserRepository.CosmosUser>>>();
        iterator.Setup(i => i.HasMoreResults).Returns(false);

        _mockContainer.Setup(c => c.GetItemQueryIterator<CosmosUserRepository.CosmosUser>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Returns(iterator.Object);

        // Act
        var result = await _repository.IsAuthorizedAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task IsAuthorizedAsync_ShouldReturnIsAuthorized(bool isAuthorized)
    {
        // Arrange
        var userId = _fixture.Create<long>();
        var user = new CosmosUserRepository.CosmosUser()
        {
            Id = userId.ToString(),
            IsAuthorized = isAuthorized,
        };
        var users = new List<CosmosUserRepository.CosmosUser> { user };
        var feedResponse = _fixture.Create<Mock<FeedResponse<CosmosUserRepository.CosmosUser>>>();
        feedResponse.Setup(r => r.GetEnumerator())
            .Returns(users.GetEnumerator());
        var iterator = _fixture.Create<Mock<FeedIterator<CosmosUserRepository.CosmosUser>>>();
        iterator.Setup(i => i.HasMoreResults).Returns(true);
        iterator.Setup(i => i.ReadNextAsync(default))
            .ReturnsAsync(feedResponse.Object);

        _mockContainer.Setup(c => c.GetItemQueryIterator<CosmosUserRepository.CosmosUser>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Returns(iterator.Object);

        // Act
        var result = await _repository.IsAuthorizedAsync(userId);

        // Assert
        result.Should().Be(isAuthorized);
    }

    [Test]
    public async Task AuthorizeUserAsync_ShouldUpsertUser()
    {
        // Arrange
        var userId = _fixture.Create<long>();

        // Act
        await _repository.AuthorizeUserAsync(userId);

        // Assert
        _mockContainer.Verify(c => c.UpsertItemAsync(
            It.Is<CosmosUserRepository.CosmosUser>(u => u.Id == userId.ToString() && u.IsAuthorized),
            It.Is<PartitionKey>(k => k == new PartitionKey(userId.ToString())),
            It.IsAny<ItemRequestOptions>(),
            default), Times.Once);
    }
}