using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.DependencyInjection;
using MiroslavGPT.AWS.Settings;
using MiroslavGPT.AWS.Threads;
using MiroslavGPT.AWS.Users;
using MiroslavGPT.Domain.Extensions;
using MiroslavGPT.Domain.Interfaces.Threads;
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
            OpenAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
            TelegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN"),
            TelegramBotUsername = Environment.GetEnvironmentVariable("TELEGRAM_BOT_USERNAME"),
            MaxTokens = int.Parse(Environment.GetEnvironmentVariable("MAX_TOKENS")!),
            RegionName = Environment.GetEnvironmentVariable("AWS_REGION"),
            UsersTableName = Environment.GetEnvironmentVariable("DYNAMODB_USERS_TABLE_NAME"),
            ThreadTableName = Environment.GetEnvironmentVariable("DYNAMODB_THREAD_TABLE_NAME"),
            ThreadLengthLimit = int.Parse(Environment.GetEnvironmentVariable("THREAD_LENGTH_LIMIT")!),
        };

        services.AddSingleton<ITelegramBotSettings>(amazonSettings);
        services.AddSingleton<IChatGptBotSettings>(amazonSettings);
        services.AddSingleton<IRegionSettings>(amazonSettings);
        services.AddSingleton<IUserSettings>(amazonSettings);
        services.AddSingleton<IThreadSettings>(amazonSettings);

        services.AddSingleton<IUserRepository, DynamoUserRepository>();
        services.AddSingleton<IThreadRepository, DynamoThreadRepository>();

        services.AddSingleton<IDynamoDBContext>(s =>
        {
            var client = new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(s.GetService<IRegionSettings>()!.RegionName));
            return new DynamoDBContext(client);
        });

        services.AddDomainServices();
    }
}