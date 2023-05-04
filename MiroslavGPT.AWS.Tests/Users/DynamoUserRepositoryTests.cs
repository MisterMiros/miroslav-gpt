using Amazon.DynamoDBv2.DataModel;
using MiroslavGPT.AWS.Settings;
using MiroslavGPT.AWS.Users;

namespace MiroslavGPT.AWS.Tests.Users;

public class DynamoUserRepositoryTests
{
    private Fixture _fixture;
    private Mock<IUserSettings> _mockSettings;
    private Mock<IDynamoDBContext> _mockContext;
    private DynamoUserRepository _repository;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _mockSettings = _fixture.Freeze<Mock<IUserSettings>>();
        _mockContext = _fixture.Freeze<Mock<IDynamoDBContext>>();
        _repository = _fixture.Create<DynamoUserRepository>();
    }

    [Test, AutoData]
    public async Task IsAuthorizedAsync_ShouldReturnFalse_WhenUserIsNull(string tableName, long chatId)
    {
        // Arrange
        _mockSettings.Setup(s => s.UsersTableName).Returns(tableName);

        _mockContext.Setup(c => c.LoadAsync<DynamoUserRepository.DynamoUser>(
                chatId,
                It.Is<DynamoDBOperationConfig>(c => c.OverrideTableName == tableName),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((DynamoUserRepository.DynamoUser)null);

        // Act
        var result = await _repository.IsAuthorizedAsync(chatId);

        // Assert
        result.Should().Be(false);
    }
    
    [Test, AutoData]
    public async Task IsAuthorizedAsync_ShouldReturnNotAuthorized(string tableName, long chatId)
    {
        // Arrange
        _mockSettings.Setup(s => s.UsersTableName).Returns(tableName);

        _mockContext.Setup(c => c.LoadAsync<DynamoUserRepository.DynamoUser>(
                chatId,
                It.Is<DynamoDBOperationConfig>(c => c.OverrideTableName == tableName),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DynamoUserRepository.DynamoUser { ChatId = chatId, Authorized = false});

        // Act
        var result = await _repository.IsAuthorizedAsync(chatId);

        // Assert
        result.Should().Be(false);
    }
    
    [Test, AutoData]
    public async Task IsAuthorizedAsync_ShouldReturnAuthorized(string tableName, long chatId)
    {
        // Arrange
        _mockSettings.Setup(s => s.UsersTableName).Returns(tableName);

        _mockContext.Setup(c => c.LoadAsync<DynamoUserRepository.DynamoUser>(
                chatId,
                It.Is<DynamoDBOperationConfig>(c => c.OverrideTableName == tableName),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DynamoUserRepository.DynamoUser { ChatId = chatId, Authorized = true});

        // Act
        var result = await _repository.IsAuthorizedAsync(chatId);

        // Assert
        result.Should().Be(true);
    }
    
    [Test, AutoData]
    public async Task AuthorizeUserAsync_ShouldWork(string tableName, long chatId)
    {
        // Arrange
        _mockSettings.Setup(s => s.UsersTableName).Returns(tableName);

        // Act
        await _repository.AuthorizeUserAsync(chatId);

        // Assert
        _mockContext.Verify(c => c.SaveAsync(
            It.Is<DynamoUserRepository.DynamoUser>(u => u.ChatId == chatId && u.Authorized == true),
            It.Is<DynamoDBOperationConfig>(c => c.OverrideTableName == tableName),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockContext.VerifyNoOtherCalls();
    }
}