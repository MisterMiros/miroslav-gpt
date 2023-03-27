using MiroslavGPT.Domain.Models;

namespace MiroslavGPT.Domain.Interfaces
{
    public interface IBot
    {
        Task<BotResponse> ProcessCommandAsync(long chatId, string username, string text);
    }
}
