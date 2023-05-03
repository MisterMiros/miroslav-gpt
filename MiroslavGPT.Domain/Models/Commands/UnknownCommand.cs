namespace MiroslavGPT.Domain.Models.Commands;

public class UnknownCommand: ICommand
{
    public long ChatId { get; set; }
    public int MessageId { get; set; }
}