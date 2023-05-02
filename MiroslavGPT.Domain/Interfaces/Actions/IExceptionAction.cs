namespace MiroslavGPT.Domain.Interfaces.Actions;

public interface IExceptionAction
{
    Task ExecuteAsync(long chatId, int messageId);
}