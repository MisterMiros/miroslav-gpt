using MiroslavGPT.Domain.Settings;

namespace MiroslavGPT.Azure.Settings
{
    public class AzureSettings : ITelegramBotSettings, IChatGptBotSettings, ICosmosDBSettings, ICosmosDBUsersSettings, IAzureSpeechSettings
    {
        public string TelegramBotUsername { get; set; }
        public string TelegramBotToken { get; set; }
        public string SecretKey { get; set; }
        public string OpenAiApiKey { get; set; }
        public int MaxTokens { get; set; }
        public string ConnectionString { get; set; }
        public string UsersDatabaseName { get; set; }
        public string UsersContainerName { get; set; }
        public string AzureSpeechRegion { get; set; }
        public string AzureSpeechKey { get; set; }
    }
}
