using MiroslavGPT.Domain.Interfaces;

namespace MiroslavGPT.Domain.Models.Commands;

public record InitCommand: ICommand
{
    public long ChatId { get; set; }
    public int MessageId { get; set; }
    public string Secret { get; set; }
}