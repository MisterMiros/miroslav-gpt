using Microsoft.AspNetCore.Mvc;
using MiroslavGPT.Admin.API.Models.Personalities;
using MiroslavGPT.Admin.Domain.Interfaces.Personalities;
using MiroslavGPT.Model.Personalities;

namespace MiroslavGPT.Admin.API.Controllers;

[ApiController]
[Route("api/personality")]
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
    public async Task<IActionResult> GetPersonalities()
    {
        var personalities = await _personalityRepository.GetPersonalitiesAsync();
        return Ok(personalities.Select(ApiPersonality.From));
    }

    /// <summary>
    /// Gets a list of personalities.
    /// </summary>
    /// <returns>A a list of personalities</returns>
    /// <remarks>
    /// </remarks>
    /// <response code="200">Returns a personality</response>
    /// <response code="404">Not found a personality by that id</response>
    [HttpGet("{id:string}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiPersonality))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPersonality([FromRoute] string id)
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
    /// <remarks>
    /// </remarks>
    /// <response code="201">Returns newly created personality</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiPersonality))]
    public async Task<IActionResult> CreatePersonality([FromBody] CreatePersonalityRequest createRequest)
    {
        var personality = await _personalityRepository.InsertPersonalityAsync(createRequest.ToPersonality());
        return CreatedAtAction(
            nameof(GetPersonality),
            new { id = personality.Id },
            ApiPersonality.From(personality)
        );
    }
}