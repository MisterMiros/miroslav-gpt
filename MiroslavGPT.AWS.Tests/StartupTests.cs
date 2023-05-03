using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.DependencyInjection;
using MiroslavGPT.Domain.Settings;
using MiroslavGPT.AWS.Settings;
using MiroslavGPT.AWS.Threads;
using MiroslavGPT.AWS.Users;
using MiroslavGPT.Domain;
using MiroslavGPT.Domain.Actions;
using MiroslavGPT.Domain.Clients;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Interfaces.Actions;
using MiroslavGPT.Domain.Interfaces.Clients;
using MiroslavGPT.Domain.Interfaces.Personality;
using MiroslavGPT.Domain.Interfaces.Threads;
using MiroslavGPT.Domain.Interfaces.Users;
using MiroslavGPT.Domain.Models.Commands;
using MiroslavGPT.Domain.Personality;
using OpenAI_API.Chat;
using Telegram.Bot;

namespace MiroslavGPT.AWS.Tests;

public class StartupTests
{
    [Test, AutoData]
    public void Startup_WorksFine(AmazonSettings amazonSettings)
    {
        // Arrange
        var services = new ServiceCollection();
        Environment.SetEnvironmentVariable("AWS_REGION", amazonSettings.RegionName);
        Environment.SetEnvironmentVariable("TELEGRAM_BOT_USERNAME", amazonSettings.TelegramBotUsername);
        Environment.SetEnvironmentVariable("TELEGRAM_BOT_TOKEN", amazonSettings.TelegramBotToken);
        Environment.SetEnvironmentVariable("SECRET_KEY", amazonSettings.SecretKey);
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", amazonSettings.OpenAiApiKey);
        Environment.SetEnvironmentVariable("MAX_TOKENS", amazonSettings.MaxTokens.ToString());
        Environment.SetEnvironmentVariable("DYNAMODB_USERS_TABLE_NAME", amazonSettings.UsersTableName);
        Environment.SetEnvironmentVariable("DYNAMODB_THREAD_TABLE_NAME", amazonSettings.ThreadTableName);
        Environment.SetEnvironmentVariable("THREAD_LENGTH_LIMIT", amazonSettings.ThreadLengthLimit.ToString());

        // Act
        Startup.ConfigureServices(services);

        // Assert
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(ITelegramBotSettings), amazonSettings));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IChatGptBotSettings), amazonSettings));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IRegionSettings), amazonSettings));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IUserSettings), amazonSettings));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IThreadSettings), amazonSettings));

        services.Should().Contain(d => d.Lifetime == ServiceLifetime.Singleton && d.ServiceType == typeof(IDynamoDBContext) && d.ImplementationFactory != null);
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IUserRepository), typeof(DynamoUserRepository), ServiceLifetime.Singleton));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IThreadRepository), typeof(DynamoThreadRepository), ServiceLifetime.Singleton));
        
        
        services.Should().Contain(d => d.Lifetime == ServiceLifetime.Singleton && d.ServiceType == typeof(IChatEndpoint) && d.ImplementationFactory != null);
        services.Should().Contain(d => d.Lifetime == ServiceLifetime.Singleton && d.ServiceType == typeof(ITelegramBotClient) && d.ImplementationFactory != null);
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(ITelegramClient), typeof(TelegramClient), ServiceLifetime.Singleton));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IChatClient), typeof(ChatClient), ServiceLifetime.Singleton));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IPersonalityProvider), typeof(PersonalityProvider), ServiceLifetime.Singleton));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IAction<InitCommand>), typeof(InitAction), ServiceLifetime.Singleton));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IAction<PromptCommand>), typeof(PromptAction), ServiceLifetime.Singleton));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IAction<UnknownCommand>), typeof(UnknownAction), ServiceLifetime.Singleton));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IExceptionAction), typeof(ExceptionAction), ServiceLifetime.Singleton));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(ITelegramMessageHandler), typeof(TelegramMessageHandler), ServiceLifetime.Singleton));
    }
}