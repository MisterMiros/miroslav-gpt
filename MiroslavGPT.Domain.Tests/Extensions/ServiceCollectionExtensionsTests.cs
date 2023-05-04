using Microsoft.Extensions.DependencyInjection;
using MiroslavGPT.Domain.Actions;
using MiroslavGPT.Domain.Clients;
using MiroslavGPT.Domain.Extensions;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Interfaces.Actions;
using MiroslavGPT.Domain.Interfaces.Clients;
using MiroslavGPT.Domain.Interfaces.Personality;
using MiroslavGPT.Domain.Models.Commands;
using MiroslavGPT.Domain.Personality;
using OpenAI_API.Chat;
using Telegram.Bot;

namespace MiroslavGPT.Domain.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddDomainServices_ShouldAddServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDomainServices();

        // Assert
        services.Should().NotBeEmpty();
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