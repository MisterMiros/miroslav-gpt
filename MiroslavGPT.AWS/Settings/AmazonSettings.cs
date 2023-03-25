using MiroslavGPT.Domain.Settings;

namespace MiroslavGPT.AWS.Settings
{
    public class AmazonSettings : ITelegramBotSettings, IChatGptBotSettings, IRegionSettings, IDynamoDBUsersSettings
    {
        public string TelegramBotUsername { get; set; }
        public string TelegramBotToken { get; set; }
        public string SecretKey { get; set; }
        public string OpenAiApiKey { get; set; }
        public int MaxTokens { get; set; }
        public string RegionName { get; set; }
        public string UsersTableName { get; set; }
    }
}
