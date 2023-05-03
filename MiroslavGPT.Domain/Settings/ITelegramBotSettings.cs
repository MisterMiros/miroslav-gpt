namespace MiroslavGPT.Domain.Settings;

public interface ITelegramBotSettings
{
    public string TelegramBotUsername { get; set; }
    public string TelegramBotToken { get; set; }
}