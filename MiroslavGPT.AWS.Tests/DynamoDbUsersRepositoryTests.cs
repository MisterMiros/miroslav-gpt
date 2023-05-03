using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using MiroslavGPT.AWS.Settings;
using MiroslavGPT.AWS.Users;

namespace MiroslavGPT.AWS.Tests
{
    public class DynamoDbUsersRepositoryTests
    {
        private Fixture _fixture = new Fixture();
        private Mock<IAmazonDynamoDB> _mockDynamoDbClient;
        private Mock<IRegionSettings> _mockRegionSettings;
        private Mock<IUserSettings> _mockDynamoDbUsersSettings;
        private DynamoUserRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());
            _mockDynamoDbClient = _fixture.Freeze<Mock<IAmazonDynamoDB>>();
            _mockRegionSettings = _fixture.Freeze<Mock<IRegionSettings>>();
            _mockDynamoDbUsersSettings = _fixture.Freeze<Mock<IUserSettings>>();

            _repository = _fixture.Create<DynamoUserRepository>();
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
            _mockDynamoDbUsersSettings.Setup(s => s.UsersTableName)
                .Returns(tableName);
            _mockDynamoDbClient.Setup(c => c.GetItemAsync(tableName, It.Is<Dictionary<string, AttributeValue>>(d => d.ContainsKey("ChatId") && d["ChatId"].N == chatId.ToString()), default))
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
            _mockDynamoDbUsersSettings.Setup(s => s.UsersTableName)
                .Returns(tableName);
            _mockDynamoDbClient.Setup(c => c.GetItemAsync(tableName, It.Is<Dictionary<string, AttributeValue>>(d => d.ContainsKey("ChatId") && d["ChatId"].N == chatId.ToString()), default))
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
            _mockDynamoDbUsersSettings.Setup(s => s.UsersTableName)
                .Returns(tableName);
            _mockDynamoDbClient.Setup(c => c.GetItemAsync(tableName, It.Is<Dictionary<string, AttributeValue>>(d => d.ContainsKey("ChatId") && d["ChatId"].N == chatId.ToString()), default))
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
            _mockDynamoDbUsersSettings.Setup(s => s.UsersTableName)
                .Returns(tableName);
            Dictionary<string, AttributeValue> expectedAuthorizedValue = null;
            _mockDynamoDbClient.Setup(c => c.PutItemAsync(tableName, It.IsAny<Dictionary<string, AttributeValue>>(), default))
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
