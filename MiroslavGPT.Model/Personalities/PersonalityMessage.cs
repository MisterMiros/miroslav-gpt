namespace MiroslavGPT.Model.Personality;

public record PersonalityMessage
{
    public string Text { get; set; } = string.Empty;
    public bool IsAssistant { get; set; }
}