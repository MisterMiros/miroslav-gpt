using Telegram.Bot.Types;

namespace MiroslavGPT.Domain.Interfaces
{
    public interface IBot
    {
        Task<string> ProcessCommandAsync(Update update);
    }
}
