namespace MiroslavGPT.Admin.API.Models.Personalities;

public record UpdatePersonalityMessageRequest
{
    public string Text { get; set; } = string.Empty;
}