namespace MiroslavGPT.Admin.API.Models.Personalities;

public class CreatePersonalityMessageRequest
{
    public string Text { get; set; } = string.Empty;
    public bool IsAssistant { get; set; }
}