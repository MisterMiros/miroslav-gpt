namespace MiroslavGPT.Domain.Interfaces
{
    public interface IBot
    {
        Task<string> ProcessCommandAsync(long chatId, string username, string text);
    }
}
