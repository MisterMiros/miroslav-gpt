using Microsoft.AspNetCore.Mvc;
using MiroslavGPT.Admin.API.Controllers;
using MiroslavGPT.Admin.API.Models.Personalities;
using MiroslavGPT.Admin.Domain.Interfaces.Personalities;
using MiroslavGPT.Model.Personalities;

namespace MiroslavGPT.Admin.API.Tests.Controllers;

[TestFixture]
public class PersonalityControllerTests
{
    private Fixture _fixture;
    private Mock<IPersonalityRepository> _mockPersonalityRepository;
    private PersonalityController _controller;

    [SetUp]
    public void SetUp()
    {
        _fixture = new();
        _fixture.Customize(new AutoMoqCustomization());
        _mockPersonalityRepository = _fixture.Freeze<Mock<IPersonalityRepository>>();
        _controller = new(_mockPersonalityRepository.Object);
    }

    [Test, AutoData]
    public async Task GetPersonalities_ReturnsOk(List<Personality> personalities)
    {
        // Arrange
        _mockPersonalityRepository.Setup(r => r.GetPersonalitiesAsync())
            .ReturnsAsync(personalities);
        
        // Act
        var result = await _controller.GetPersonalitiesAsync();
        
        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(personalities.Select(ApiPersonality.From));
    }
    
    [Test]
    public async Task GetPersonalities_ReturnsOk_WhenEmptyList()
    {
        // Arrange
        _mockPersonalityRepository.Setup(r => r.GetPersonalitiesAsync())
            .ReturnsAsync(new List<Personality>());
        
        // Act
        var result = await _controller.GetPersonalitiesAsync();
        
        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeAssignableTo<IEnumerable<ApiPersonality>>().Which.Should().BeEmpty();
    }

    [Test, AutoData]
    public async Task GetPersonalityAsync_ReturnsOk(Personality personality)
    {
        // Arrange
        _mockPersonalityRepository.Setup(r => r.GetPersonalityAsync(personality.Id))
            .ReturnsAsync(personality);
        
        // Act
        var result = await _controller.GetPersonalityAsync(personality.Id);
        
        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(ApiPersonality.From(personality));
    }
    
    [Test, AutoData]
    public async Task GetPersonalityAsync_ReturnsNotFound(Personality personality)
    {
        // Arrange
        _mockPersonalityRepository.Setup(r => r.GetPersonalityAsync(personality.Id))
            .ReturnsAsync((Personality)null);
        
        // Act
        var result = await _controller.GetPersonalityAsync(personality.Id);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
    
    [Test, AutoData]
    public async Task CreatePersonalityAsync_ReturnsCreated(Personality personality, CreatePersonalityRequest createRequest)
    {
        // Arrange
        _mockPersonalityRepository.Setup(r => r.CreatePersonalityAsync(createRequest.Command))
            .ReturnsAsync(personality);
        
        // Act
        var result = await _controller.CreatePersonalityAsync(createRequest);
        
        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(ApiPersonality.From(personality));
        createdResult.ActionName.Should().Be("GetPersonality");
    }
    
    [Test, AutoData]
    public async Task UpdatePersonalityAsync_ReturnsNoContent(string id, UpdatePersonalityRequest updateRequest)
    {
        // Arrange
        _mockPersonalityRepository.Setup(r => r.UpdatePersonalityAsync(id, updateRequest.Command));

        // Act
        var result = await _controller.UpdatePersonalityAsync(id, updateRequest);
        
        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockPersonalityRepository.Verify();
    }
}