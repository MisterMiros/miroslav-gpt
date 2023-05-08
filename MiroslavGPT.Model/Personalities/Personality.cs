using MiroslavGPT.Model.Personality;

namespace MiroslavGPT.Model.Personalities;

public record Personality
{
    public string Id { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public List<PersonalityMessage> Messages { get; set; } = new();
}