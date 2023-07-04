using Microsoft.AspNetCore.Mvc;
using MiroslavGPT.Admin.API.Models.Personalities;
using MiroslavGPT.Admin.Domain.Interfaces.Personalities;
using MiroslavGPT.Model.Personalities;

namespace MiroslavGPT.Admin.API.Controllers;

[ApiController]
[Route("api/personalities")]
public class PersonalityController : ControllerBase
{
    private readonly IPersonalityRepository _personalityRepository;

    public PersonalityController(IPersonalityRepository personalityRepository)
    {
        _personalityRepository = personalityRepository;
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
        var personalities = await _personalityRepository.GetPersonalitiesAsync();
        return Ok(personalities.Select(ApiPersonality.From));
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
        var personality = await _personalityRepository.GetPersonalityAsync(id);
        if (personality == null)
        {
            return NotFound();
        }

        return Ok(ApiPersonality.From(personality));
    }

    /// <summary>
    /// Gets an empty personality
    /// </summary>
    /// <returns>Newly created personality</returns>
    /// <response code="201">Returns newly created personality</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiPersonality))]
    public async Task<IActionResult> CreatePersonalityAsync([FromBody] CreatePersonalityRequest createRequest)
    {
        var personality = await _personalityRepository.CreatePersonalityAsync(createRequest.Command);
        return CreatedAtAction(
            "GetPersonality",
            new { id = personality.Id },
            ApiPersonality.From(personality)
        );
    }
    
    /// <summary>
    /// Updates personality command
    /// </summary>
    /// <response code="204">Indicates successful update</response>
    [HttpPut("{id:string}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdatePersonalityAsync(string id, [FromBody] UpdatePersonalityRequest updateRequest)
    {
        await _personalityRepository.UpdatePersonalityAsync(id, updateRequest.Command);
        return NoContent();
    }
    
    /// <summary>
    /// Updates personality command
    /// </summary>
    /// <response code="201">Newly created message with reference to containing personality</response>
    [HttpPut("{id:string}/messages")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddPersonalityMessageAsync(string id, [FromBody] CreatePersonalityMessageRequest updateRequest)
    {
        var message = await _personalityRepository.AddPersonalityMessageAsync(id, updateRequest.Text, updateRequest.IsAssistant);
        return CreatedAtAction(
            "GetPersonality",
            new { id },
            ApiPersonalityMessage.From(message)
        );
    }
    
    /// <summary>
    /// Deletes message from personality
    /// </summary>
    /// <response code="204">Indicates successful delete</response>
    [HttpDelete("{id:string}/messages/{messageId:string}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeletePersonalityMessageAsync(string id, string messageId)
    {
        await _personalityRepository.DeletePersonalityMessageAsync(id, messageId);
        return NoContent();
    }
    
}