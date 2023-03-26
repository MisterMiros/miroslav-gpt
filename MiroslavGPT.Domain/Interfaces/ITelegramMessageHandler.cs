using Telegram.Bot.Types;

namespace MiroslavGPT.Domain.Interfaces
{
    public interface ITelegramMessageHandler
    {
        Task ProcessUpdateAsync(Update update);
    }
}
