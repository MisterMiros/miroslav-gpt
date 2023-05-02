using MiroslavGPT.Domain.Settings;

namespace MiroslavGPT.Azure.Settings
{
    public class AzureSettings : ITelegramBotSettings, IChatGptBotSettings, ICosmosDbSettings, ICosmosDbUsersSettings
    {
        public string TelegramBotUsername { get; set; }
        public string TelegramBotToken { get; set; }
        public string SecretKey { get; set; }
        public string OpenAiApiKey { get; set; }
        public int MaxTokens { get; set; }
        public string ConnectionString { get; set; }
        public string UsersDatabaseName { get; set; }
        public string UsersContainerName { get; set; }
    }
}
