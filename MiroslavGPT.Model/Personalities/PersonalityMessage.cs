namespace MiroslavGPT.Model.Personalities;

public record PersonalityMessage
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsAssistant { get; set; }
}