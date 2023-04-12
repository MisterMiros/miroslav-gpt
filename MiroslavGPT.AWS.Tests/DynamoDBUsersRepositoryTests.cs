using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using MiroslavGPT.AWS.Factories;
using MiroslavGPT.AWS.Settings;

namespace MiroslavGPT.AWS.Tests
{
    public class DynamoDBUsersRepositoryTests
    {
        private Fixture _fixture = new Fixture();
        private Mock<IDynamoDBClientFactory> _mockDynamoDBClientFactory;
        private Mock<IAmazonDynamoDB> _mockDynamoDBClient;
        private Mock<IRegionSettings> _mockRegionSettings;
        private Mock<IDynamoDBUsersSettings> _mockDynamoDBUsersSettings;
        private DynamoDBUsersRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());
            _mockDynamoDBClientFactory = _fixture.Freeze<Mock<IDynamoDBClientFactory>>();
            _mockDynamoDBClient = _fixture.Freeze<Mock<IAmazonDynamoDB>>();
            _mockRegionSettings = _fixture.Freeze<Mock<IRegionSettings>>();
            _mockDynamoDBUsersSettings = _fixture.Freeze<Mock<IDynamoDBUsersSettings>>();
            _mockDynamoDBClientFactory.Setup(x => x.CreateClient(_mockRegionSettings.Object.RegionName))
                .Returns(_mockDynamoDBClient.Object);

            _repository = _fixture.Create<DynamoDBUsersRepository>();
        }

        [Test]
        public async Task IsAuthorizedAsync_ShouldReturnFalse_WhenDocumentIsNull()
        {
            // Arrange
            var chatId = _fixture.Create<long>();
            var tableName = _fixture.Create<string>();
            var itemResponse = _fixture.Build<GetItemResponse>()
                .Without(r => r.Item)
                .Create();
            itemResponse.Item = null;
            _mockDynamoDBUsersSettings.Setup(s => s.UsersTableName)
                .Returns(tableName);
            _mockDynamoDBClient.Setup(c => c.GetItemAsync(tableName, It.Is<Dictionary<string, AttributeValue>>(d => d.ContainsKey("ChatId") && d["ChatId"].N == chatId.ToString()), default))
                .ReturnsAsync(itemResponse);
            
            // Act
            var result = await _repository.IsAuthorizedAsync(chatId);
            
            // Assert
            result.Should().Be(false);
        }

        [Test]
        public async Task IsAuthorizedAsync_ShouldReturnFalse_WhenNoAuthorizedField()
        {
            // Arrange
            var chatId = _fixture.Create<long>();
            var tableName = _fixture.Create<string>();
            var itemResponse = _fixture.Build<GetItemResponse>()
                .Without(r => r.Item)
                .Create();
            itemResponse.Item = new Dictionary<string, AttributeValue>();
            _mockDynamoDBUsersSettings.Setup(s => s.UsersTableName)
                .Returns(tableName);
            _mockDynamoDBClient.Setup(c => c.GetItemAsync(tableName, It.Is<Dictionary<string, AttributeValue>>(d => d.ContainsKey("ChatId") && d["ChatId"].N == chatId.ToString()), default))
                .ReturnsAsync(itemResponse);

            // Act
            var result = await _repository.IsAuthorizedAsync(chatId);

            // Assert
            result.Should().Be(false);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task IsAuthorizedAsync_ShouldReturnAuthorizedValue(bool isAuthorized)
        {
            // Arrange
            var chatId = _fixture.Create<long>();
            var tableName = _fixture.Create<string>();
            var itemResponse = _fixture.Build<GetItemResponse>()
                .Without(r => r.Item)
                .Create();
            itemResponse.Item = new Dictionary<string, AttributeValue>
            {
                { "Authorized", new AttributeValue { BOOL = isAuthorized } }
            };
            _mockDynamoDBUsersSettings.Setup(s => s.UsersTableName)
                .Returns(tableName);
            _mockDynamoDBClient.Setup(c => c.GetItemAsync(tableName, It.Is<Dictionary<string, AttributeValue>>(d => d.ContainsKey("ChatId") && d["ChatId"].N == chatId.ToString()), default))
                .ReturnsAsync(itemResponse);

            // Act
            var result = await _repository.IsAuthorizedAsync(chatId);

            // Assert
            result.Should().Be(isAuthorized);
        }

        [Test]
        public async Task AuthorizeUserAsync_ShouldWork()
        {
            // Arrange
            var chatId = _fixture.Create<long>();
            var tableName = _fixture.Create<string>();
            _mockDynamoDBUsersSettings.Setup(s => s.UsersTableName)
                .Returns(tableName);
            Dictionary<string, AttributeValue> expectedAuthorizedValue = null;
            _mockDynamoDBClient.Setup(c => c.PutItemAsync(tableName, It.IsAny<Dictionary<string, AttributeValue>>(), default))
                .Callback((string n, Dictionary<string, AttributeValue> v, CancellationToken t) =>
                {
                    expectedAuthorizedValue = v;
                });

            // Act
            await _repository.AuthorizeUserAsync(chatId);

            // Assert
            expectedAuthorizedValue.Should().NotBeNullOrEmpty();
            expectedAuthorizedValue.Should().ContainKey("ChatId")
                .WhoseValue.N.Should().Be(chatId.ToString());

            expectedAuthorizedValue.Should().ContainKey("Authorized")
                .WhoseValue.N.Should().Be("1");
        }
    }
}
