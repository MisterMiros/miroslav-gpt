﻿using Microsoft.Azure.Cosmos;
using MiroslavGPT.Azure.Factories;
using MiroslavGPT.Azure.Settings;

namespace MiroslavGPT.Azure.Tests
{
    public class CosmosDBUsersRepositoryTests
    {
        private Fixture _fixture;
        private Mock<ICosmosClientFactory> _mockCosmosClientFactory;
        private Mock<ICosmosDBSettings> _mockCosmosDBSettings;
        private Mock<ICosmosDBUsersSettings> _mockCosmosDBUsersSettings;
        private Mock<CosmosClient> _mockCosmosClient;
        private Mock<Container> _mockContainer;
        private CosmosDBUsersRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());


            _mockContainer = _fixture.Freeze<Mock<Container>>();
            _mockCosmosClient = _fixture.Freeze<Mock<CosmosClient>>();
            _mockCosmosClient.Setup(c => c.GetContainer(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_mockContainer.Object);
            _mockCosmosClientFactory = _fixture.Freeze<Mock<ICosmosClientFactory>>();
            _mockCosmosClientFactory.Setup(x => x.CreateCosmosClient(It.IsAny<string>()))
                .Returns(_mockCosmosClient.Object);
            _mockCosmosDBSettings = _fixture.Freeze<Mock<ICosmosDBSettings>>();
            _mockCosmosDBUsersSettings = _fixture.Freeze<Mock<ICosmosDBUsersSettings>>();

            _repository = _fixture.Create<CosmosDBUsersRepository>();
        }

        [Test]
        public async Task IsAuthorizedAsync_ShouldReturnFalse_WhenNotFoundException()
        {
            // Arrange
            var userId = _fixture.Create<long>();
            _mockContainer.Setup(c => c.GetItemQueryIterator<CosmosDBUsersRepository.User>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
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
            var iterator = _fixture.Create<Mock<FeedIterator<CosmosDBUsersRepository.User>>>();
            iterator.Setup(i => i.HasMoreResults).Returns(false);

            _mockContainer.Setup(c => c.GetItemQueryIterator<CosmosDBUsersRepository.User>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
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
            var user = new CosmosDBUsersRepository.User(userId.ToString(), isAuthorized);
            var users = new List<CosmosDBUsersRepository.User> { user };
            var feedResponse = _fixture.Create<Mock<FeedResponse<CosmosDBUsersRepository.User>>>();
            feedResponse.Setup(r => r.GetEnumerator())
                .Returns(users.GetEnumerator());
            var iterator = _fixture.Create<Mock<FeedIterator<CosmosDBUsersRepository.User>>>();
            iterator.Setup(i => i.HasMoreResults).Returns(true);
            iterator.Setup(i => i.ReadNextAsync(default))
                .ReturnsAsync(feedResponse.Object);

            _mockContainer.Setup(c => c.GetItemQueryIterator<CosmosDBUsersRepository.User>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
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
                It.Is<CosmosDBUsersRepository.User>(u => u.id == userId.ToString() && u.isAuthorized), 
                It.Is<PartitionKey>(k => k == new PartitionKey(userId.ToString())), 
                It.IsAny<ItemRequestOptions>(), 
                default), Times.Once);
        }
    }
}
