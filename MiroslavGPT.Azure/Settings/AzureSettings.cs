using MiroslavGPT.Domain.Settings;

namespace MiroslavGPT.Azure.Settings;

public class AzureSettings : ITelegramBotSettings, IChatGptBotSettings, ICosmosSettings, IUserSettings, IThreadSettings
{
    public string TelegramBotUsername { get; set; }
    public string TelegramBotToken { get; set; }
    public string SecretKey { get; set; }
    public string OpenAiApiKey { get; set; }
    public int MaxTokens { get; set; }
    public string ConnectionString { get; set; }
    public string UserDatabaseName { get; set; }
    public string UserContainerName { get; set; }
    public string ThreadDatabaseName { get; set; }
    public string ThreadContainerName { get; set; }
    public int ThreadLengthLimit { get; set; }
}