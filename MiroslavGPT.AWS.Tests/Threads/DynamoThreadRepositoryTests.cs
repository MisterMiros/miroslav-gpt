using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using MiroslavGPT.AWS.Settings;
using MiroslavGPT.AWS.Threads;
using MiroslavGPT.Domain.Models.Threads;

namespace MiroslavGPT.AWS.Tests.Threads;

[TestFixture]
public class DynamoThreadRepositoryTests
{
    private Fixture _fixture;
    private Mock<IThreadSettings> _mockSettings;
    private Mock<IDynamoDBContext> _mockContext;
    private DynamoThreadRepository _repository;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _mockSettings = _fixture.Freeze<Mock<IThreadSettings>>();
        _mockContext = _fixture.Freeze<Mock<IDynamoDBContext>>();
        _repository = _fixture.Create<DynamoThreadRepository>();
    }

    [Test, AutoData]
    public async Task CreateThreadAsync_ShouldCreate(long chatId, string tableName)
    {
        // Arrange
        _mockSettings.Setup(s => s.ThreadTableName)
            .Returns(tableName);
        
        // Act
        var thread = await _repository.CreateThreadAsync(chatId);

        // Assert
        thread.Should().NotBeNull();
        thread.ChatId.Should().Be(chatId);
        thread.Messages.Should().NotBeNull().And.BeEmpty();

        _mockContext.Verify(c => c.SaveAsync(
                It.Is<DynamoThreadRepository.DynamoThread>(t => t.Id == thread.Id && t.ChatId == thread.ChatId && t.Messages != null && t.Messages.Count == 0),
                It.Is<DynamoDBOperationConfig>(c => c.OverrideTableName == tableName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test, AutoData]
    public async Task GetThreadByMessageAsync_ShouldReturnThread(string tableName, long chatId, int messageId, DynamoThreadRepository.DynamoThread dynamoThread)
    {
        // Arrange
        _mockSettings.Setup(s => s.ThreadTableName)
            .Returns(tableName);

        var mockSearch = new Mock<AsyncSearch<DynamoThreadRepository.DynamoThread>>();
        mockSearch.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DynamoThreadRepository.DynamoThread>() { dynamoThread });

        var conditions = new List<ScanCondition>();
        _mockContext.Setup(c => c.ScanAsync<DynamoThreadRepository.DynamoThread>(
                It.IsAny<ScanCondition[]>(),
                It.Is<DynamoDBOperationConfig>(c => c.OverrideTableName == tableName))
            )
            .Callback((IEnumerable<ScanCondition> c, DynamoDBOperationConfig _) => conditions.AddRange(c))
            .Returns(mockSearch.Object);

        // Act
        var thread = await _repository.GetThreadByMessageAsync(chatId, messageId);

        // Assert
        thread.Should().NotBeNull();
        thread.Id.Should().Be(dynamoThread.Id);
        thread.ChatId.Should().Be(dynamoThread.ChatId);
        thread.Messages.Should().BeEquivalentTo(dynamoThread.Messages);

        conditions.Should().HaveCount(2);
        conditions[0].Should().BeEquivalentTo(new ScanCondition("ChatId", ScanOperator.Equal, chatId));
        conditions[1].Should().BeEquivalentTo(new ScanCondition("MessageIds", ScanOperator.Contains, messageId));
    }

    [Test, AutoData]
    public async Task UpdateThreadAsync_ShouldSave(string tableName, MessageThread messageThread)
    {
        // Arrange
        _mockSettings.Setup(s => s.ThreadTableName)
            .Returns(tableName);
        
        // Act
        await _repository.UpdateThreadAsync(messageThread);
        
        // Assert
        _mockContext.Verify(c => c.SaveAsync(
                It.Is<DynamoThreadRepository.DynamoThread>(t => t.Id == messageThread.Id && t.ChatId == messageThread.ChatId && t.Messages == messageThread.Messages),
                It.Is<DynamoDBOperationConfig>(c => c.OverrideTableName == tableName),
                It.IsAny<CancellationToken>()),
            Times.Once);
        
    }
}