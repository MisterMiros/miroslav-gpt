using MiroslavGPT.Model.Personalities;

namespace MiroslavGPT.Admin.API.Models.Personalities;

public record CreatePersonalityRequest
{
    public string Command { get; set; } = string.Empty;
    
    public Personality ToPersonality()
    {
        return new()
        {
            Command = Command
        };
    }
}