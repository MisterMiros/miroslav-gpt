using Telegram.Bot;

namespace MiroslavGPT.Domain.Factories
{
    public class TelegramClientFactory : ITelegramClientFactory
    {
        public ITelegramBotClient CreateBotClient(string token)
        {
            return new TelegramBotClient(token);
        }
    }
}
