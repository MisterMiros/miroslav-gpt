using MiroslavGPT.Domain.Interfaces;

namespace MiroslavGPT.Domain.Models.Commands;

public record PromptCommand: ICommand
{
    public long ChatId { get; set; }
    public int MessageId { get; set; }
    public string Personality { get; set; }
    public string Username { get; set; }
    public string Prompt { get; set; }
    public int? ReplyToId { get; set; }
}