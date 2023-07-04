using Microsoft.Azure.Cosmos;
using MiroslavGPT.Admin.Domain.Azure.Personalities;
using MiroslavGPT.Admin.Domain.Interfaces.Settings;
using MiroslavGPT.Model.Personalities;

namespace MiroslavGPT.Admin.Domain.Azure.Tests.Personalities;

[TestFixture]
public class CosmosPersonalityRepositoryTests
{
    private Fixture _fixture = null!;
    private Mock<IPersonalitySettings> _mockSettings = null!;
    private Mock<CosmosClient> _mockCosmosClient = null!;
    private Mock<Container> _mockContainer = null!;
    private CosmosPersonalityRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new();
        _fixture.Customize(new AutoMoqCustomization());

        _mockContainer = _fixture.Freeze<Mock<Container>>();
        _mockCosmosClient = _fixture.Freeze<Mock<CosmosClient>>();
        _mockCosmosClient.Setup(c => c.GetContainer(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(_mockContainer.Object);
        _mockSettings = _fixture.Freeze<Mock<IPersonalitySettings>>();

        _repository = new(_mockCosmosClient.Object, _mockSettings.Object);
    }

    private static bool PersonalitiesEqual(CosmosPersonalityRepository.CosmosPersonality cosmosPersonality, Personality personality)
    {
        return cosmosPersonality.Id == personality.Id &&
               cosmosPersonality.Command == personality.Command &&
               cosmosPersonality.Messages.Count == personality.Messages.Count &&
               cosmosPersonality.Messages.Zip(personality.Messages).All(pair =>
                   pair.First.Text == pair.Second.Text &&
                   pair.First.IsAssistant == pair.Second.IsAssistant
               );
    }

    [Test]
    public async Task GetPersonalitiesAsync_ReturnsEmptyList_WhenNotFound()
    {
        // Arrange
        _mockContainer.Setup(c => c.GetItemQueryIterator<CosmosPersonalityRepository.CosmosPersonality>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Throws(new CosmosException("Not found", System.Net.HttpStatusCode.NotFound, 0, "", 0));

        // Act
        var result = await _repository.GetPersonalitiesAsync();

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
        _mockContainer.VerifyAll();
        _mockContainer.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GetPersonalitiesAsync_ReturnsEmptyList_WhenNoValuesInIterator()
    {
        // Arrange
        var iterator = _fixture.Create<Mock<FeedIterator<CosmosPersonalityRepository.CosmosPersonality>>>();
        iterator.Setup(i => i.HasMoreResults).Returns(false);

        _mockContainer.Setup(c => c.GetItemQueryIterator<CosmosPersonalityRepository.CosmosPersonality>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Returns(iterator.Object);

        // Act
        var result = await _repository.GetPersonalitiesAsync();

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
        iterator.VerifyAll();
        iterator.VerifyNoOtherCalls();
        _mockContainer.VerifyAll();
        _mockContainer.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task GetPersonalitiesAsync_ReturnsPersonalities(
        List<CosmosPersonalityRepository.CosmosPersonality> firstPack,
        List<CosmosPersonalityRepository.CosmosPersonality> secondPack
    )
    {
        // Arrange
        var feedResponse = _fixture.Create<Mock<FeedResponse<CosmosPersonalityRepository.CosmosPersonality>>>();
        feedResponse.SetupSequence(r => r.GetEnumerator())
            .Returns(firstPack.GetEnumerator())
            .Returns(secondPack.GetEnumerator());

        var iterator = _fixture.Create<Mock<FeedIterator<CosmosPersonalityRepository.CosmosPersonality>>>();
        iterator.SetupSequence(i => i.HasMoreResults)
            .Returns(true)
            .Returns(true)
            .Returns(false);
        iterator.Setup(i => i.ReadNextAsync(default))
            .ReturnsAsync(feedResponse.Object);

        _mockContainer.Setup(c => c.GetItemQueryIterator<CosmosPersonalityRepository.CosmosPersonality>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Returns(iterator.Object);

        // Act
        var result = await _repository.GetPersonalitiesAsync();

        // Assert
        result.Should().NotBeNull()
            .And.NotBeEmpty()
            .And.HaveCount(firstPack.Count + secondPack.Count);
        firstPack.Concat(secondPack).Zip(result).Should().AllSatisfy(pair =>
            PersonalitiesEqual(pair.First, pair.Second).Should().BeTrue()
        );
        iterator.VerifyAll();
        iterator.VerifyNoOtherCalls();
        feedResponse.VerifyAll();
        feedResponse.VerifyNoOtherCalls();
        _mockContainer.VerifyAll();
        _mockContainer.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task GetPersonalityAsync_ReturnsNull_WhenNotFound(string id)
    {
        // Arrange
        _mockContainer.Setup(r => r.ReadItemAsync<CosmosPersonalityRepository.CosmosPersonality>(id, new(id), null, default))
            .ThrowsAsync(new CosmosException("Not found", System.Net.HttpStatusCode.NotFound, 0, "", 0));

        // Act
        var result = await _repository.GetPersonalityAsync(id);

        // Assert
        result.Should().BeNull();
        _mockContainer.VerifyAll();
        _mockContainer.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task GetPersonalityAsync_ReturnsPersonality(string id, CosmosPersonalityRepository.CosmosPersonality personality)
    {
        // Arrange
        var mockItemResponse = new Mock<ItemResponse<CosmosPersonalityRepository.CosmosPersonality>>();
        mockItemResponse.Setup(r => r.Resource).Returns(personality);
        _mockContainer.Setup(r => r.ReadItemAsync<CosmosPersonalityRepository.CosmosPersonality>(id, new(id), null, default))
            .ReturnsAsync(mockItemResponse.Object);

        // Act
        var result = await _repository.GetPersonalityAsync(id);

        // Assert
        result.Should().NotBeNull().And.Match<Personality>(r => PersonalitiesEqual(personality, r));
        mockItemResponse.VerifyAll();
        mockItemResponse.VerifyNoOtherCalls();
        _mockContainer.VerifyAll();
        _mockContainer.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task GetPersonalityByCommandAsync_ReturnsNull_WhenNotFound(string command)
    {
        // Arrange
        _mockContainer.Setup(c => c.GetItemQueryIterator<CosmosPersonalityRepository.CosmosPersonality>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Throws(new CosmosException("Not found", System.Net.HttpStatusCode.NotFound, 0, "", 0));

        // Act
        var result = await _repository.GetPersonalityByCommandAsync(command);

        // Assert
        result.Should().BeNull();
        _mockContainer.VerifyAll();
        _mockContainer.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task GetPersonalityByCommandAsync_ReturnsNull_WhenNoValuesInIterator(string command)
    {
        // Arrange
        var iterator = _fixture.Create<Mock<FeedIterator<CosmosPersonalityRepository.CosmosPersonality>>>();
        iterator.Setup(i => i.HasMoreResults).Returns(false);

        _mockContainer.Setup(c => c.GetItemQueryIterator<CosmosPersonalityRepository.CosmosPersonality>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Returns(iterator.Object);

        // Act
        var result = await _repository.GetPersonalityByCommandAsync(command);

        // Assert
        result.Should().BeNull();
        iterator.VerifyAll();
        iterator.VerifyNoOtherCalls();
        _mockContainer.VerifyAll();
        _mockContainer.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task GetPersonalityByCommandAsync_ReturnsPersonality(string command, CosmosPersonalityRepository.CosmosPersonality personality)
    {
        // Arrange
        var personalities = new List<CosmosPersonalityRepository.CosmosPersonality> { personality };
        var feedResponse = _fixture.Create<Mock<FeedResponse<CosmosPersonalityRepository.CosmosPersonality>>>();
        feedResponse.Setup(r => r.GetEnumerator())
            .Returns(personalities.GetEnumerator());

        var iterator = _fixture.Create<Mock<FeedIterator<CosmosPersonalityRepository.CosmosPersonality>>>();
        iterator.Setup(i => i.HasMoreResults)
            .Returns(true);
        iterator.Setup(i => i.ReadNextAsync(default))
            .ReturnsAsync(feedResponse.Object);

        _mockContainer.Setup(c => c.GetItemQueryIterator<CosmosPersonalityRepository.CosmosPersonality>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Returns(iterator.Object);

        // Act
        var result = await _repository.GetPersonalityByCommandAsync(command);

        // Assert
        result.Should().NotBeNull().And.Match<Personality>(r => PersonalitiesEqual(personality, r));
        iterator.VerifyAll();
        iterator.VerifyNoOtherCalls();
        feedResponse.VerifyAll();
        feedResponse.VerifyNoOtherCalls();
        _mockContainer.VerifyAll();
        _mockContainer.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task InsertPersonalityAsync_InsertsPersonality(string command)
    {
        // Arrange
        var cosmosResult = _fixture.Create<CosmosPersonalityRepository.CosmosPersonality>();
        var mockItemResponse = new Mock<ItemResponse<CosmosPersonalityRepository.CosmosPersonality>>();
        mockItemResponse.Setup(r => r.Resource).Returns(cosmosResult);

        CosmosPersonalityRepository.CosmosPersonality? upsertedItem = null;
        _mockContainer.Setup(c => c.CreateItemAsync(It.IsAny<CosmosPersonalityRepository.CosmosPersonality>(), It.IsAny<PartitionKey>(), null, default))
            .Callback((CosmosPersonalityRepository.CosmosPersonality p, PartitionKey? _, ItemRequestOptions __, CancellationToken ___) => { upsertedItem = p; }).ReturnsAsync(mockItemResponse.Object);

        // Act
        var result = await _repository.CreatePersonalityAsync(command);

        // Assert
        result.Should().NotBeNull().And.Match<Personality>(r => PersonalitiesEqual(cosmosResult, r));
        upsertedItem.Should().NotBeNull();
        upsertedItem!.Id.Should().NotBeEmpty();
        upsertedItem.Should().NotBeNull().And.Match<CosmosPersonalityRepository.CosmosPersonality>(r => r.Command == command && r.Messages.Count == 0);

        mockItemResponse.VerifyAll();
        mockItemResponse.VerifyNoOtherCalls();
        _mockContainer.VerifyAll();
        _mockContainer.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task UpdatePersonalityAsync_UpdatesPersonality(string id, string command)
    {
        // Arrange
        // Act
        await _repository.UpdatePersonalityAsync(id, command);

        // Assert
        _mockContainer.Verify(c => c.PatchItemAsync<CosmosPersonalityRepository.CosmosPersonality>(
            id,
            new(id),
            It.Is<IReadOnlyList<PatchOperation>>(l => l.Count == 1
                                                      && l[0].OperationType == PatchOperationType.Replace
                                                      && l[0].Path == "/command"),
            null,
            default), Times.Once);
        _mockContainer.VerifyNoOtherCalls();
    }
    
    [Test, AutoData]
    public async Task UpdatePersonalityAsync_UpdatesPersonality(string id)
    {
        // Arrange
        // Act
        await _repository.DeletePersonalityAsync(id);

        // Assert
        _mockContainer.Verify(c => c.DeleteItemAsync<CosmosPersonalityRepository.CosmosPersonality>(
            id,
            new(id),
            default,
            default), Times.Once);
        _mockContainer.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task AddPersonalityMessageAsync_AddsMessage(string id, string text, bool isAssistant)
    {
        // Arrange
        // Act
        var message = await _repository.AddPersonalityMessageAsync(id, text, isAssistant);

        // Assert
        message.Should().NotBeNull();
        message.Text.Should().Be(text);
        message.IsAssistant.Should().Be(isAssistant);
        _mockContainer.Verify(c => c.PatchItemAsync<CosmosPersonalityRepository.CosmosPersonality>(
            id,
            new(id),
            It.Is<IReadOnlyList<PatchOperation>>(l => l.Count == 1
                                                      && l[0].OperationType == PatchOperationType.Add
                                                      && l[0].Path == "/messages/-"),
            null,
            default), Times.Once);
        _mockContainer.VerifyNoOtherCalls();
    }
    
    [Test, AutoData]
    public async Task UpdatePersonalityMessageAsync_UpdatesMessage(string id, string messageId, string text)
    {
        // Arrange
        // Act
        await _repository.UpdatePersonalityMessageAsync(id, messageId, text);

        // Assert
        _mockContainer.Verify(c => c.PatchItemAsync<CosmosPersonalityRepository.CosmosPersonality>(
            id,
            new(id),
            It.Is<IReadOnlyList<PatchOperation>>(l => l.Count == 1
                                                      && l[0].OperationType == PatchOperationType.Replace
                                                      && l[0].Path == $"/messages/[@id='{messageId}']/text"),
            null,
            default), Times.Once);
        _mockContainer.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task DeletePersonalityMessageAsync_DeletesMessage(string id, string messageId)
    {
        // Arrange
        // Act
        await _repository.DeletePersonalityMessageAsync(id, messageId);

        // Assert
        _mockContainer.Verify(c => c.PatchItemAsync<CosmosPersonalityRepository.CosmosPersonality>(
            id,
            new(id),
            It.Is<IReadOnlyList<PatchOperation>>(l => l.Count == 1
                                                      && l[0].OperationType == PatchOperationType.Remove
                                                      && l[0].Path == $"/messages/[@id='{messageId}']"),
            null,
            default), Times.Once);
        _mockContainer.VerifyNoOtherCalls();
    }
}