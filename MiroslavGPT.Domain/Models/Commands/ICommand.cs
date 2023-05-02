namespace MiroslavGPT.Domain.Models.Commands;

public interface ICommand
{
    public long ChatId { get; set; }
    public int MessageId { get; set; }
}