namespace MiroslavGPT.Admin.API.Models.Personalities;

public record UpdatePersonalityRequest
{
    public string Command { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}