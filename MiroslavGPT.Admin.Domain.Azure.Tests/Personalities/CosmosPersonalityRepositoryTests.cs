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
    }
}