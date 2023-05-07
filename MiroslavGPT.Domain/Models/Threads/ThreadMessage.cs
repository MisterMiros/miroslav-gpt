namespace MiroslavGPT.Domain.Models.Threads;

public record ThreadMessage
{
    public int MessageId { get; set; }
    public string? Username { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsAssistant { get; set; }
}