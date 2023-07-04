using MiroslavGPT.Model.Personalities;

namespace MiroslavGPT.Admin.API.Models.Personalities;

public record ApiPersonality
{
    public string Id { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string SystemMessage { get; set; } = string.Empty;
    public List<ApiPersonalityMessage> Messages { get; set; } = new();
    
    public static ApiPersonality From(Personality personality)
    {
        return new()
        {
            Id = personality.Id,
            Command = personality.Command,
            SystemMessage = personality.SystemMessage,
            Messages = personality.Messages.Select(ApiPersonalityMessage.From).ToList()
        };
    }
}