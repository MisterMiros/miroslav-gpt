namespace MiroslavGPT.Domain.Settings
{
    public interface ITelegramBotSettings
    {
        public string TelegramBotName { get; set; }
        public string TelegramBotToken { get; set; }
    }
}
