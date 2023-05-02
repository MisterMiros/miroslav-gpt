using Microsoft.Extensions.DependencyInjection;
using MiroslavGPT.AWS.Factories;
using MiroslavGPT.AWS.Settings;
using MiroslavGPT.Domain.Extensions;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Interfaces.Users;
using MiroslavGPT.Domain.Settings;

namespace MiroslavGPT.AWS;

public class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var amazonSettings = new AmazonSettings
        {
            SecretKey = Environment.GetEnvironmentVariable("SECRET_KEY"),
            OpenAiApiKey= Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
            TelegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN"),
            TelegramBotUsername = Environment.GetEnvironmentVariable("TELEGRAM_BOT_USERNAME"),
            MaxTokens = int.Parse(Environment.GetEnvironmentVariable("MAX_TOKENS") ?? "100"),
            RegionName = Environment.GetEnvironmentVariable("AWS_REGION"),
            UsersTableName = Environment.GetEnvironmentVariable("DYNAMODB_USERS_TABLE_NAME"),
        };

        services.AddSingleton<ITelegramBotSettings>(amazonSettings);
        services.AddSingleton<IChatGptBotSettings>(amazonSettings);
        services.AddSingleton<IRegionSettings>(amazonSettings);
        services.AddSingleton<IDynamoDbUsersSettings>(amazonSettings);

        services.AddSingleton<IUsersRepository, DynamoDbUsersRepository>();
        services.AddSingleton<IDynamoDbClientFactory, DynamoDbClientFactory>();

        services.AddDomainServices();
    }
}