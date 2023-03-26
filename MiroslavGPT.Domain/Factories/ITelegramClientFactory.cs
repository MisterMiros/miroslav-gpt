namespace MiroslavGPT.Domain.Factories
{
    public interface ITelegramClientFactory
    {
        public Telegram.Bot.ITelegramBotClient CreateBotClient(string token);
    }
}
