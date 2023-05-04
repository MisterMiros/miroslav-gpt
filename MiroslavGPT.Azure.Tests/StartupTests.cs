using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiroslavGPT.Azure.Settings;
using MiroslavGPT.Azure.Threads;
using MiroslavGPT.Azure.Users;
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
using MiroslavGPT.Domain.Settings;
using OpenAI_API.Chat;
using Telegram.Bot;

namespace MiroslavGPT.Azure.Tests;

public class StartupTests
{
    private Fixture _fixture;
    private Startup _startup;
    private Mock<IWebJobsBuilder> _mockBuilder;
    private Mock<IServiceCollection> _mockServiceCollection;
    private Mock<IConfiguration> _mockConfiguration;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
        _mockServiceCollection = _fixture.Freeze<Mock<IServiceCollection>>();
        _mockConfiguration = _fixture.Freeze<Mock<IConfiguration>>();
        _mockBuilder = _fixture.Freeze<Mock<IWebJobsBuilder>>();
        _mockBuilder.Setup(b => b.Services)
            .Returns(_mockServiceCollection.Object);
        _startup = new Startup();
    }

    [Test, AutoData]
    public void Startup_WorksFine(AzureSettings azureSettings)
    {
        // Arrange
        List<ServiceDescriptor> services = new List<ServiceDescriptor>();
        _mockServiceCollection.Setup(c => c.Add(It.IsAny<ServiceDescriptor>()))
            .Callback((ServiceDescriptor d) =>
            {
                services.Add(d);
            });
        _mockConfiguration.Setup(c => c["TELEGRAM_BOT_USERNAME"])
            .Returns(azureSettings.TelegramBotUsername);
        _mockConfiguration.Setup(c => c["TELEGRAM_BOT_TOKEN"])
            .Returns(azureSettings.TelegramBotToken);
        _mockConfiguration.Setup(c => c["SECRET_KEY"])
            .Returns(azureSettings.SecretKey);
        _mockConfiguration.Setup(c => c["OPENAI_API_KEY"])
            .Returns(azureSettings.OpenAiApiKey);
        _mockConfiguration.Setup(c => c["MAX_TOKENS"])
            .Returns(azureSettings.MaxTokens.ToString());
        _mockConfiguration.Setup(c => c["COSMOSDB_CONNECTION_STRING"])
            .Returns(azureSettings.ConnectionString);
        _mockConfiguration.Setup(c => c["COSMOSDB_DATABASE_NAME"])
            .Returns(azureSettings.UserDatabaseName);
        _mockConfiguration.Setup(c => c["COSMOSDB_CONTAINER_NAME"])
            .Returns(azureSettings.UserContainerName);
        _mockConfiguration.Setup(c => c["COSMOSDB_THREAD_DATABASE_NAME"])
            .Returns(azureSettings.ThreadDatabaseName);
        _mockConfiguration.Setup(c => c["COSMOSDB_THREAD_CONTAINER_NAME"])
            .Returns(azureSettings.ThreadContainerName);
        _mockConfiguration.Setup(c => c["THREAD_LENGTH_LIMIT"])
            .Returns(azureSettings.ThreadLengthLimit.ToString());

        // Act
        _startup.Configure(new WebJobsBuilderContext
        {
            Configuration = _mockConfiguration.Object,
        }, _mockBuilder.Object);


        // Assert
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(ITelegramBotSettings), azureSettings));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IChatGptBotSettings), azureSettings));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(ICosmosSettings), azureSettings));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IUserSettings), azureSettings));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IThreadSettings), azureSettings));

        services.Should().Contain(d => d.Lifetime == ServiceLifetime.Singleton && d.ServiceType == typeof(CosmosClient) && d.ImplementationFactory != null);
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IUserRepository), typeof(CosmosUserRepository), ServiceLifetime.Singleton));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IThreadRepository), typeof(CosmosThreadRepository), ServiceLifetime.Singleton));

        services.Should().Contain(d => d.Lifetime == ServiceLifetime.Singleton && d.ServiceType == typeof(IChatEndpoint) && d.ImplementationFactory != null);
        services.Should().Contain(d => d.Lifetime == ServiceLifetime.Singleton && d.ServiceType == typeof(ITelegramBotClient) && d.ImplementationFactory != null);
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(ITelegramClient), typeof(TelegramClient), ServiceLifetime.Singleton));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IChatClient), typeof(ChatClient), ServiceLifetime.Singleton));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IPersonalityProvider), typeof(PersonalityProvider), ServiceLifetime.Singleton));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IAction), typeof(InitAction), ServiceLifetime.Singleton));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IAction), typeof(PromptAction), ServiceLifetime.Singleton));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IAction), typeof(UnknownAction), ServiceLifetime.Singleton));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IExceptionAction), typeof(ExceptionAction), ServiceLifetime.Singleton));
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(ITelegramMessageHandler), typeof(TelegramMessageHandler), ServiceLifetime.Singleton));
    }
}