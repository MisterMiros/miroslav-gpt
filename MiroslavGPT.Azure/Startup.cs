﻿using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MiroslavGPT.Azure.Settings;
using MiroslavGPT.Azure.Threads;
using MiroslavGPT.Azure.Users;
using MiroslavGPT.Domain.Extensions;
using MiroslavGPT.Domain.Interfaces.Threads;
using MiroslavGPT.Domain.Interfaces.Users;
using MiroslavGPT.Domain.Settings;

[assembly: FunctionsStartup(typeof(MiroslavGPT.Azure.Startup))]

namespace MiroslavGPT.Azure;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var config = builder.GetContext().Configuration;

        var azureSettings = new AzureSettings
        {
            TelegramBotUsername = config["TELEGRAM_BOT_USERNAME"],
            TelegramBotToken = config["TELEGRAM_BOT_TOKEN"],
            SecretKey = config["SECRET_KEY"],
            OpenAiApiKey = config["OPENAI_API_KEY"],
            MaxTokens = int.Parse(config["MAX_TOKENS"]),
            ConnectionString = config["COSMOSDB_CONNECTION_STRING"],
            UserDatabaseName = config["COSMOSDB_DATABASE_NAME"],
            UserContainerName = config["COSMOSDB_CONTAINER_NAME"],
            ThreadDatabaseName = config["COSMOSDB_THREAD_DATABASE_NAME"],
            ThreadContainerName = config["COSMOSDB_THREAD_CONTAINER_NAME"],
            ThreadLengthLimit = int.Parse(config["THREAD_LENGTH_LIMIT"]),
        };

        builder.Services.AddSingleton<ITelegramBotSettings>(azureSettings);
        builder.Services.AddSingleton<IChatGptBotSettings>(azureSettings);
        builder.Services.AddSingleton<ICosmosSettings>(azureSettings);
        builder.Services.AddSingleton<IUserSettings>(azureSettings);
        builder.Services.AddSingleton<IThreadSettings>(azureSettings);

        builder.Services.AddSingleton(s => new CosmosClient(s.GetService<ICosmosSettings>()!.ConnectionString));
        
        builder.Services.AddSingleton<IUserRepository, CosmosUserRepository>();
        builder.Services.AddSingleton<IThreadRepository, CosmosThreadRepository>();

        builder.Services.AddLogging();

        builder.Services.AddDomainServices();
    }
}