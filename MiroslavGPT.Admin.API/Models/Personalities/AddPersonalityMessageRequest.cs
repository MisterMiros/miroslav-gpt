namespace MiroslavGPT.Admin.API.Models.Personalities;

public record AddPersonalityMessageRequest
{
    public string Text { get; set; } = string.Empty;
    public bool IsAssistant { get; set; }
}