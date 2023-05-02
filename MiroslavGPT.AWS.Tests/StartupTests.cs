using Castle.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiroslavGPT.Domain.Factories;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Settings;
using MiroslavGPT.Domain;
using MiroslavGPT.AWS.Settings;
using MiroslavGPT.AWS.Factories;
using MiroslavGPT.Domain.Interfaces.Personality;
using MiroslavGPT.Domain.Interfaces.Users;
using MiroslavGPT.Domain.Personality;

namespace MiroslavGPT.AWS.Tests;

public class StartupTests
{
    private Fixture _fixture;
    private Startup _startup;
    private Mock<IServiceCollection> _mockServiceCollection;
    private Mock<IConfiguration> _mockConfiguration;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
        _mockServiceCollection = _fixture.Freeze<Mock<IServiceCollection>>();
        _startup = new Startup();
    }

    [Test, AutoData]
    public void Startup_WorksFine(AmazonSettings amazonSettings)
    {
        // Arrange
        List<ServiceDescriptor> serviceDescriptors = new List<ServiceDescriptor>();
        _mockServiceCollection.Setup(c => c.Add(It.IsAny<ServiceDescriptor>()))
            .Callback((ServiceDescriptor d) =>
            {
                serviceDescriptors.Add(d);
            });
        Environment.SetEnvironmentVariable("TELEGRAM_BOT_USERNAME", amazonSettings.TelegramBotUsername);
        Environment.SetEnvironmentVariable("TELEGRAM_BOT_TOKEN", amazonSettings.TelegramBotToken);
        Environment.SetEnvironmentVariable("SECRET_KEY", amazonSettings.SecretKey);
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", amazonSettings.OpenAiApiKey);
        Environment.SetEnvironmentVariable("MAX_TOKENS", amazonSettings.MaxTokens.ToString());
        Environment.SetEnvironmentVariable("DYNAMODB_USERS_TABLE_NAME", amazonSettings.UsersTableName);
        Environment.SetEnvironmentVariable("AWS_REGION", amazonSettings.RegionName);

        // Act
        Startup.ConfigureServices(_mockServiceCollection.Object);

        // Assert
        serviceDescriptors.Should().HaveCount(11);

        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(ITelegramBotSettings))
            .Which.ImplementationInstance.Should().BeOfType<AmazonSettings>()
            .Which.Should().BeEquivalentTo(amazonSettings);

        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(IChatGptBotSettings))
            .Which.ImplementationInstance.Should().BeOfType<AmazonSettings>()
            .Which.Should().BeEquivalentTo(amazonSettings);

        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(IDynamoDBUsersSettings))
            .Which.ImplementationInstance.Should().BeOfType<AmazonSettings>()
            .Which.Should().BeEquivalentTo(amazonSettings);

        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(IRegionSettings))
            .Which.ImplementationInstance.Should().BeOfType<AmazonSettings>()
            .Which.Should().BeEquivalentTo(amazonSettings);

        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(IUsersRepository) &&
                                                 d.ImplementationType == typeof(DynamoDBUsersRepository) &&
                                                 d.Lifetime == ServiceLifetime.Singleton);

        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(IDynamoDBClientFactory) &&
                                                 d.ImplementationType == typeof(DynamoDBClientFactory) &&
                                                 d.Lifetime == ServiceLifetime.Singleton);

        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(ITelegramClientFactory) &&
                                                 d.ImplementationType == typeof(TelegramClientFactory) &&
                                                 d.Lifetime == ServiceLifetime.Singleton);

        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(IOpenAiClientFactory) &&
                                                 d.ImplementationType == typeof(OpenAiClientFactory) &&
                                                 d.Lifetime == ServiceLifetime.Singleton);

        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(IPersonalityProvider) &&
                                                 d.ImplementationType == typeof(PersonalityProvider) &&
                                                 d.Lifetime == ServiceLifetime.Singleton);

        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(IBot) &&
                                                 d.ImplementationType == typeof(ChatGPTBot) &&
                                                 d.Lifetime == ServiceLifetime.Singleton);

        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(ITelegramMessageHandler) &&
                                                 d.ImplementationType == typeof(TelegramMessageHandler) &&
                                                 d.Lifetime == ServiceLifetime.Singleton);
    }

    [Test, AutoData]
    public void Startup_SetsDefaultMaxTokens_WhenNoneProvided(AmazonSettings amazonSettings)
    {
        // Arrange
        List<ServiceDescriptor> serviceDescriptors = new List<ServiceDescriptor>();
        _mockServiceCollection.Setup(c => c.Add(It.IsAny<ServiceDescriptor>()))
            .Callback((ServiceDescriptor d) =>
            {
                serviceDescriptors.Add(d);
            });
        Environment.SetEnvironmentVariable("TELEGRAM_BOT_USERNAME", amazonSettings.TelegramBotUsername);
        Environment.SetEnvironmentVariable("TELEGRAM_BOT_TOKEN", amazonSettings.TelegramBotToken);
        Environment.SetEnvironmentVariable("SECRET_KEY", amazonSettings.SecretKey);
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", amazonSettings.OpenAiApiKey);
        Environment.SetEnvironmentVariable("MAX_TOKENS", null);
        Environment.SetEnvironmentVariable("DYNAMODB_USERS_TABLE_NAME", amazonSettings.UsersTableName);
        Environment.SetEnvironmentVariable("AWS_REGION", amazonSettings.RegionName);


        // Act
        Startup.ConfigureServices(_mockServiceCollection.Object);

        // Assert
        serviceDescriptors.Should().Contain(d => d.ServiceType == typeof(IChatGptBotSettings))
            .Which.ImplementationInstance.Should().BeOfType<AmazonSettings>()
            .Which.MaxTokens.Should().Be(100);
    }
}