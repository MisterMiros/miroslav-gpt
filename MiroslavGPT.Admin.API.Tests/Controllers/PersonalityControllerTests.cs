using Microsoft.AspNetCore.Mvc;
using MiroslavGPT.Admin.API.Controllers;
using MiroslavGPT.Admin.API.Models.Personalities;
using MiroslavGPT.Admin.Domain.Common;
using MiroslavGPT.Admin.Domain.Errors;
using MiroslavGPT.Admin.Domain.Interfaces.Personalities;
using MiroslavGPT.Model.Personalities;

namespace MiroslavGPT.Admin.API.Tests.Controllers;

[TestFixture]
public class PersonalityControllerTests
{
    private Fixture _fixture = null!;
    private Mock<IPersonalityService> _mockPersonalityService = null!;
    private PersonalityController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new();
        _fixture.Customize(new AutoMoqCustomization());
        _mockPersonalityService = _fixture.Freeze<Mock<IPersonalityService>>();
        _controller = new(_mockPersonalityService.Object);
    }

    [Test, AutoData]
    public async Task GetPersonalitiesAsync_ReturnsOk(List<Personality> personalities)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.GetPersonalitiesResultAsync())
            .ReturnsAsync(Result<List<Personality>>.Ok(personalities));

        // Act
        var result = await _controller.GetPersonalitiesAsync();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(personalities.Select(ApiPersonality.From));
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GetPersonalities_ReturnsOk_WhenEmptyList()
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.GetPersonalitiesResultAsync())
            .ReturnsAsync(Result<List<Personality>>.Ok(new()));

        // Act
        var result = await _controller.GetPersonalitiesAsync();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeAssignableTo<IEnumerable<ApiPersonality>>().Which.Should().BeEmpty();
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GetPersonalitiesAsync_Throws_WhenFailed()
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.GetPersonalitiesResultAsync())
            .ReturnsAsync(Result<List<Personality>>.Failure("I failed"));

        // Act
        var ex = Assert.ThrowsAsync<InvalidOperationException>(_controller.GetPersonalitiesAsync);

        // Assert
        ex.Should().NotBeNull();
        ex.Message.Should().Be($"Failed to get personalities. Error: I failed");
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task GetPersonalityAsync_ReturnsOk(string id, Personality personality)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.GetPersonalityResultAsync(id))
            .ReturnsAsync(Result<Personality>.Ok(personality));

        // Act
        var result = await _controller.GetPersonalityAsync(id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<ApiPersonality>().And.BeEquivalentTo(ApiPersonality.From(personality));
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task GetPersonalityAsync_ReturnsNotFound(string id)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.GetPersonalityResultAsync(id))
            .ReturnsAsync(Result<Personality>.Failure(PersonalityError.NOT_FOUND));

        // Act
        var result = await _controller.GetPersonalityAsync(id);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task GetPersonalityAsync_Throws_WhenFailed(string id)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.GetPersonalityResultAsync(id))
            .ReturnsAsync(Result<Personality>.Failure("I failed"));

        // Act
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _controller.GetPersonalityAsync(id));

        // Assert
        ex.Should().NotBeNull();
        ex.Message.Should().Be($"Failed to get personality. Error: I failed");
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task CreatePersonalityAsync_ReturnsOk(CreatePersonalityRequest request, Personality personality)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.CreatePersonalityResultAsync(request.Command))
            .ReturnsAsync(Result<Personality>.Ok(personality));

        // Act
        var result = await _controller.CreatePersonalityAsync(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var okResult = result as CreatedAtActionResult;
        okResult!.Value.Should().BeOfType<ApiPersonality>().And.BeEquivalentTo(ApiPersonality.From(personality));
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task CreatePersonalityAsync_ReturnsConflict(CreatePersonalityRequest request)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.CreatePersonalityResultAsync(request.Command))
            .ReturnsAsync(Result<Personality>.Failure(PersonalityError.ALREADY_EXISTS));

        // Act
        var result = await _controller.CreatePersonalityAsync(request);

        // Assert
        result.Should().BeOfType<ConflictResult>();
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task CreatePersonalityAsync_ReturnsBadRequest_WhenEmptyCommand(CreatePersonalityRequest request)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.CreatePersonalityResultAsync(request.Command))
            .ReturnsAsync(Result<Personality>.Failure(PersonalityError.EMPTY_COMMAND));

        // Act
        var result = await _controller.CreatePersonalityAsync(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which.Value!.Should().Be("Command should not be empty");
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }
    
    [Test, AutoData]
    public async Task CreatePersonalityAsync_ReturnsBadRequest_WhenInvalidCommand(CreatePersonalityRequest request)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.CreatePersonalityResultAsync(request.Command))
            .ReturnsAsync(Result<Personality>.Failure(PersonalityError.INVALID_COMMAND));

        // Act
        var result = await _controller.CreatePersonalityAsync(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which.Value!.Should().Be("Command should match start with '/' and contain only letters and numbers");
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }
    
    [Test, AutoData]
    public async Task CreatePersonalityAsync_Throws_WhenFailed(CreatePersonalityRequest request)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.CreatePersonalityResultAsync(request.Command))
            .ReturnsAsync(Result<Personality>.Failure("I failed"));

        // Act
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _controller.CreatePersonalityAsync(request));

        // Assert
        ex.Should().NotBeNull();
        ex.Message.Should().Be($"Failed to create personality. Error: I failed");
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }
    
    [Test, AutoData]
    public async Task UpdatePersonalityAsync_ReturnsOk(string id, UpdatePersonalityRequest request)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.UpdatePersonalityResultAsync(id, request.Command, request.SystemMessage))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _controller.UpdatePersonalityAsync(id, request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }
    
    [Test, AutoData]
    public async Task UpdatePersonalityAsync_ReturnsNotFound(string id, UpdatePersonalityRequest request)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.UpdatePersonalityResultAsync(id ,request.Command, request.SystemMessage))
            .ReturnsAsync(Result.Failure(PersonalityError.NOT_FOUND));

        // Act
        var result = await _controller.UpdatePersonalityAsync(id, request);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task UpdatePersonalityAsync_ReturnsConflict(string id, UpdatePersonalityRequest request)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.UpdatePersonalityResultAsync(id ,request.Command, request.SystemMessage))
            .ReturnsAsync(Result.Failure(PersonalityError.ALREADY_EXISTS));

        // Act
        var result = await _controller.UpdatePersonalityAsync(id, request);

        // Assert
        result.Should().BeOfType<ConflictResult>();
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task UpdatePersonalityAsync_ReturnsBadRequest_WhenEmptyCommand(string id, UpdatePersonalityRequest request)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.UpdatePersonalityResultAsync(id ,request.Command, request.SystemMessage))
            .ReturnsAsync(Result.Failure(PersonalityError.EMPTY_COMMAND));

        // Act
        var result = await _controller.UpdatePersonalityAsync(id, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which.Value!.Should().Be("Command should not be empty");
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }
    
    [Test, AutoData]
    public async Task UpdatePersonalityAsync_ReturnsBadRequest_WhenInvalidCommand(string id, UpdatePersonalityRequest request)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.UpdatePersonalityResultAsync(id, request.Command, request.SystemMessage))
            .ReturnsAsync(Result.Failure(PersonalityError.INVALID_COMMAND));

        // Act
        var result = await _controller.UpdatePersonalityAsync(id, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which.Value!.Should().Be("Command should match start with '/' and contain only letters and numbers");
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }
    
    [Test, AutoData]
    public async Task UpdatePersonalityAsync_Throws_WhenFailed(string id, UpdatePersonalityRequest request)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.UpdatePersonalityResultAsync(id ,request.Command, request.SystemMessage))
            .ReturnsAsync(Result.Failure("I failed"));

        // Act
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _controller.UpdatePersonalityAsync(id, request));

        // Assert
        ex.Should().NotBeNull();
        ex.Message.Should().Be($"Failed to update personality. Error: I failed");
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }
    
    [Test, AutoData]
    public async Task AddPersonalityMessageAsync_ReturnsOk(string id, AddPersonalityMessageRequest request, PersonalityMessage message)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.AddPersonalityMessageResultAsync(id, request.Text, request.IsAssistant))
            .ReturnsAsync(Result<PersonalityMessage>.Ok(message));

        // Act
        var result = await _controller.AddPersonalityMessageAsync(id, request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var okResult = result as CreatedAtActionResult;
        okResult!.Value.Should().BeOfType<ApiPersonalityMessage>().And.BeEquivalentTo(ApiPersonalityMessage.From(message));
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task AddPersonalityMessageAsync_ReturnsNotFound(string id, AddPersonalityMessageRequest request)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.AddPersonalityMessageResultAsync(id, request.Text, request.IsAssistant))
            .ReturnsAsync(Result<PersonalityMessage>.Failure(PersonalityError.NOT_FOUND));

        // Act
        var result = await _controller.AddPersonalityMessageAsync(id, request);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public async Task AddPersonalityMessageAsync_ReturnsBadRequest_WhenEmptyText(string id, AddPersonalityMessageRequest request)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.AddPersonalityMessageResultAsync(id, request.Text, request.IsAssistant))
            .ReturnsAsync(Result<PersonalityMessage>.Failure(PersonalityError.EMPTY_MESSAGE));

        // Act
        var result = await _controller.AddPersonalityMessageAsync(id, request);
        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which.Value!.Should().Be("Message should not be empty");
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }
    
    [Test, AutoData]
    public async Task AddPersonalityMessageAsync_Throws_WhenFailed(string id, AddPersonalityMessageRequest request)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.AddPersonalityMessageResultAsync(id, request.Text, request.IsAssistant))
            .ReturnsAsync(Result<PersonalityMessage>.Failure("I failed"));

        // Act
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _controller.AddPersonalityMessageAsync(id, request));

        // Assert
        ex.Should().NotBeNull();
        ex.Message.Should().Be($"Failed to add message to personality. Error: I failed");
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }
    
    [Test, AutoData]
    public async Task UpdatePersonalityMessageAsync_ReturnsOk(string id, string messageId, UpdatePersonalityMessageRequest request)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.UpdatePersonalityMessageResultAsync(id, messageId, request.Text))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _controller.UpdatePersonalityMessageAsync(id, messageId, request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }
    
    [Test, AutoData]
    public async Task UpdatePersonalityMessageAsync_ReturnsNotFound(string id, string messageId, UpdatePersonalityMessageRequest request)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.UpdatePersonalityMessageResultAsync(id, messageId, request.Text))
            .ReturnsAsync(Result.Failure(PersonalityError.NOT_FOUND));

        // Act
        var result = await _controller.UpdatePersonalityMessageAsync(id, messageId, request);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }
    
    [Test, AutoData]
    public async Task UpdatePersonalityMessageAsync_ReturnsBadRequest_WhenEmptyText(string id, string messageId, UpdatePersonalityMessageRequest request)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.UpdatePersonalityMessageResultAsync(id, messageId, request.Text))
            .ReturnsAsync(Result.Failure(PersonalityError.EMPTY_MESSAGE));

        // Act
        var result = await _controller.UpdatePersonalityMessageAsync(id, messageId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which.Value!.Should().Be("Message should not be empty");
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }
    
    [Test, AutoData]
    public async Task UpdatePersonalityMessageAsync_Throws_WhenFailed(string id, string messageId, UpdatePersonalityMessageRequest request)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.UpdatePersonalityMessageResultAsync(id, messageId, request.Text))
            .ReturnsAsync(Result.Failure("I failed"));

        // Act
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _controller.UpdatePersonalityMessageAsync(id, messageId, request));

        // Assert
        ex.Should().NotBeNull();
        ex.Message.Should().Be($"Failed to update message in personality. Error: I failed");
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }
    
    [Test, AutoData]
    public async Task DeletePersonalityMessageAsync_ReturnsOk(string id, string messageId)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.DeletePersonalityMessageResultAsync(id, messageId))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _controller.DeletePersonalityMessageAsync(id, messageId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }
    
    [Test, AutoData]
    public async Task DeletePersonalityMessageAsync_Throws_WhenFailed(string id, string messageId)
    {
        // Arrange
        _mockPersonalityService.Setup(r => r.DeletePersonalityMessageResultAsync(id, messageId))
            .ReturnsAsync(Result.Failure("I failed"));

        // Act
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _controller.DeletePersonalityMessageAsync(id, messageId));

        // Assert
        ex.Should().NotBeNull();
        ex.Message.Should().Be($"Failed to delete message from personality. Error: I failed");
        _mockPersonalityService.VerifyAll();
        _mockPersonalityService.VerifyNoOtherCalls();
    }
}