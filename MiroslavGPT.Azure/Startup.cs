using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MiroslavGPT.Azure.Settings;
using MiroslavGPT.Domain;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Personalities;
using MiroslavGPT.Domain.Settings;

[assembly: FunctionsStartup(typeof(MiroslavGPT.Azure.Startup))]

namespace MiroslavGPT.Azure
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = builder.GetContext().Configuration;

            var azureSettings = new AzureSettings
            {
                TelegramBotName = config["TELEGRAM_BOT_USERNAME"],
                TelegramBotToken = config["TELEGRAM_BOT_TOKEN"],
                SecretKey = config["SECRET_KEY"],
                OpenAiApiKey = config["OPENAI_API_KEY"],
                MaxTokens = int.Parse(config["MAX_TOKENS"] ?? "100"),
                ConnectionString = config["COSMOSDB_CONNECTION_STRING"],
                UsersDatabaseName = config["COSMOSDB_DATABASE_NAME"],
                UsersContainerName = config["COSMOSDB_CONTAINER_NAME"]
            };

            builder.Services.AddSingleton<ITelegramBotSettings>(azureSettings);
            builder.Services.AddSingleton<IChatGptBotSettings>(azureSettings);
            builder.Services.AddSingleton<ICosmosDBSettings>(azureSettings);
            builder.Services.AddSingleton<ICosmosDBUsersSettings>(azureSettings);

            builder.Services.AddSingleton<IUsersRepository, CosmosDBUsersRepository>();
            builder.Services.AddSingleton<IPersonalityProvider, TsunderePersonalityProvider>();
            builder.Services.AddSingleton<ChatGPTBot>();
            builder.Services.AddSingleton<TelegramMessageHandler>();
        }
    }
}