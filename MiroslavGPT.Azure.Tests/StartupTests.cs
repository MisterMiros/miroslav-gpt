using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiroslavGPT.Azure.Settings;
using MiroslavGPT.Azure.Users;
using MiroslavGPT.Domain;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Interfaces.Personality;
using MiroslavGPT.Domain.Interfaces.Users;
using MiroslavGPT.Domain.Personality;
using MiroslavGPT.Domain.Settings;

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
        List<ServiceDescriptor> serviceDescriptors = new List<ServiceDescriptor>();
        _mockServiceCollection.Setup(c => c.Add(It.IsAny<ServiceDescriptor>()))
            .Callback((ServiceDescriptor d) =>
            {
                serviceDescriptors.Add(d);
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

        // Act
        _startup.Configure(new WebJobsBuilderContext
        {
            Configuration = _mockConfiguration.Object,
        }, _mockBuilder.Object);


        // Assert
        serviceDescriptors.Should().HaveCount(11);

        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(ITelegramBotSettings))
            .Which.ImplementationInstance.Should().BeOfType<AzureSettings>()
            .Which.Should().BeEquivalentTo(azureSettings);

        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(IChatGptBotSettings))
            .Which.ImplementationInstance.Should().BeOfType<AzureSettings>()
            .Which.Should().BeEquivalentTo(azureSettings);

        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(ICosmosSettings))
            .Which.ImplementationInstance.Should().BeOfType<AzureSettings>()
            .Which.Should().BeEquivalentTo(azureSettings);

        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(ICosmosUserSettings))
            .Which.ImplementationInstance.Should().BeOfType<AzureSettings>()
            .Which.Should().BeEquivalentTo(azureSettings);

        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(IUserRepository) &&
                                                 d.ImplementationType == typeof(CosmosUserRepository) &&
                                                 d.Lifetime == ServiceLifetime.Singleton);

        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(IPersonalityProvider) &&
                                                 d.ImplementationType == typeof(PersonalityProvider) &&
                                                 d.Lifetime == ServiceLifetime.Singleton);

        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(ITelegramMessageHandler) &&
                                                 d.ImplementationType == typeof(TelegramMessageHandler) &&
                                                 d.Lifetime == ServiceLifetime.Singleton);
    }

    [Test, AutoData]
    public void Startup_SetsDefaultMaxTokens_WhenNoneProvided(AzureSettings azureSettings)
    {
        // Arrange
        List<ServiceDescriptor> serviceDescriptors = new List<ServiceDescriptor>();
        _mockServiceCollection.Setup(c => c.Add(It.IsAny<ServiceDescriptor>()))
            .Callback((ServiceDescriptor d) =>
            {
                serviceDescriptors.Add(d);
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
            .Returns((string)null);
        _mockConfiguration.Setup(c => c["COSMOSDB_CONNECTION_STRING"])
            .Returns(azureSettings.ConnectionString);
        _mockConfiguration.Setup(c => c["COSMOSDB_DATABASE_NAME"])
            .Returns(azureSettings.UserDatabaseName);
        _mockConfiguration.Setup(c => c["COSMOSDB_CONTAINER_NAME"])
            .Returns(azureSettings.UserContainerName);

        // Act
        _startup.Configure(new WebJobsBuilderContext
        {
            Configuration = _mockConfiguration.Object,
        }, _mockBuilder.Object);


        // Assert
        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(IChatGptBotSettings))
            .Which.ImplementationInstance.Should().BeOfType<AzureSettings>()
            .Which.MaxTokens.Should().Be(100);
    }
}