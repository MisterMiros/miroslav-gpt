using System.Net;
using Microsoft.AspNetCore.Mvc;
using MiroslavGPT.Admin.API.Models.Personalities;
using MiroslavGPT.Admin.Domain.Errors;
using MiroslavGPT.Admin.Domain.Interfaces.Personalities;
using MiroslavGPT.Model.Personalities;

namespace MiroslavGPT.Admin.API.Controllers;

[ApiController]
[Route("api/personalities")]
public class PersonalityController : ControllerBase
{
    private readonly IPersonalityService _personalityService;

    public PersonalityController(IPersonalityService personalityService)
    {
        _personalityService = personalityService;
    }

    /// <summary>
    /// Gets a list of personalities.
    /// </summary>
    /// <returns>A a list of personalities</returns>
    /// <remarks>
    /// </remarks>
    /// <response code="200">Returns a list of personalities</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ApiPersonality>))]
    public async Task<IActionResult> GetPersonalitiesAsync()
    {
        var result = await _personalityService.GetPersonalitiesResultAsync();
        if (!result.Success)
        {
            return Problem("Failed to get personalities");
        }
        return Ok(result.Value!.Select(ApiPersonality.From));
    }

    /// <summary>
    /// Gets a list of personalities.
    /// </summary>
    /// <returns>A a list of personalities</returns>
    /// <response code="200">Returns a personality</response>
    /// <response code="404">Not found a personality by that id</response>
    [HttpGet("{id:string}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiPersonality))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPersonalityAsync([FromRoute] string id)
    {
        var result = await _personalityService.GetPersonalityResultAsync(id);
        if (!result.Success)
        {
            return result.Error switch
            {
                PersonalityError.NOT_FOUND => NotFound(),
                _ => Problem("Failed to get personality")
            };
        }

        return Ok(ApiPersonality.From(result.Value!));
    }

    /// <summary>
    /// Gets an empty personality
    /// </summary>
    /// <returns>Newly created personality</returns>
    /// <response code="201">Returns newly created personality</response>
    /// <response code="400">Invalid command</response>
    /// <response code="409">Personality with that command already exists</response>
    [HttpPut("{id:string}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiPersonality))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreatePersonalityAsync([FromBody] CreatePersonalityRequest createRequest)
    {
        var result = await _personalityService.CreatePersonalityResultAsync(createRequest.Command);
        if (!result.Success)
        {
            return result.Error switch
            {
                PersonalityError.ALREADY_EXISTS => Conflict(),
                PersonalityError.INVALID_COMMAND => BadRequest("Command should match start with '/' and contain only letters and numbers"),
                PersonalityError.EMPTY_COMMAND => BadRequest("Command should not be empty"),
                _ => Problem("Failed to create personality")
            };
        }
        return CreatedAtAction(
            "GetPersonality",
            new { id = result.Value!.Id },
            ApiPersonality.From(result.Value)
        );
    }
    
    /// <summary>
    /// Updates personality command and/or system message
    /// </summary>
    /// <response code="204">Indicates successful update</response>
    /// <response code="404">Not found a personality by that id</response>
    /// <response code="400">Invalid command</response>
    /// <response code="409">Personality with that command already exists</response>
    [HttpPut("{id:string}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdatePersonalityAsync(string id, [FromBody] UpdatePersonalityRequest updateRequest)
    {
        var result = await _personalityService.UpdatePersonalityResultAsync(id, updateRequest.Command, updateRequest.Message);
        if (!result.Success)
        {
            return result.Error switch
            {
                PersonalityError.ALREADY_EXISTS => Conflict(),
                PersonalityError.INVALID_COMMAND => BadRequest("Command should match start with '/' and contain only letters and numbers"),
                PersonalityError.EMPTY_COMMAND => BadRequest("Command should not be empty"),
                _ => Problem("Failed to update personality")
            };
        }
        return NoContent();
    }
    
    /// <summary>
    /// Add new message to personality
    /// </summary>
    /// <response code="201">Newly created message with reference to containing personality</response>
    /// <response code="404">Not found a personality by that id</response>
    /// <response code="400">Message should not be empty</response>
    [HttpPut("{id:string}/messages")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddPersonalityMessageAsync(string id, [FromBody] CreatePersonalityMessageRequest updateRequest)
    {
        var result = await _personalityService.AddPersonalityMessageResultAsync(id, updateRequest.Text, updateRequest.IsAssistant);
        if (!result.Success)
        {
            return result.Error switch
            {
                PersonalityError.NOT_FOUND => NotFound(),
                PersonalityError.EMPTY_MESSAGE => BadRequest("Message should not be empty"),
                _ => Problem("Failed to add message to personality")
            };
        }
        return CreatedAtAction(
            "GetPersonality",
            new { id },
            ApiPersonalityMessage.From(result.Value!)
        );
    }
    
    
    
    /// <summary>
    /// Updates message in personality
    /// </summary>
    /// <response code="204">Indicates successful update</response>
    /// <response code="404">Not found a personality or message by given ids</response>
    /// <response code="400">Message should not be empty</response>
    [HttpPut("{id:string}/messages/{messageId:string}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePersonalityMessageAsync(string id, string messageId, [FromBody] UpdatePersonalityMessageRequest updateRequest)
    {
        var result = await _personalityService.UpdatePersonalityMessageResultAsync(id, messageId, updateRequest.Text);
        if (!result.Success)
        {
            return result.Error switch
            {
                PersonalityError.NOT_FOUND => NotFound(),
                PersonalityError.EMPTY_MESSAGE => BadRequest("Message should not be empty"),
                _ => Problem("Failed to update message in personality")
            };
        }
        return NoContent();
    }
    
    /// <summary>
    /// Deletes message from personality
    /// </summary>
    /// <response code="204">Indicates successful delete</response>
    [HttpDelete("{id:string}/messages/{messageId:string}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeletePersonalityMessageAsync(string id, string messageId)
    {
        var result = await _personalityService.DeletePersonalityMessageResultAsync(id, messageId);
        if (!result.Success)
        {
            return Problem("Failed to delete message from personality");
        }
        return NoContent();
    }
    
}