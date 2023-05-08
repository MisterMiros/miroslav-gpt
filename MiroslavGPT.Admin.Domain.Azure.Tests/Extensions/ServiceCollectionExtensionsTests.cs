using Microsoft.Extensions.DependencyInjection;
using MiroslavGPT.Admin.Domain.Azure.Extensions;
using MiroslavGPT.Admin.Domain.Azure.Personalities;
using MiroslavGPT.Admin.Domain.Interfaces.Personalities;

namespace MiroslavGPT.Admin.Domain.Azure.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddDomainServices_ShouldAddServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAzureAdminDomainServices();

        // Assert
        services.Should().NotBeEmpty();
        services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IPersonalityRepository), typeof(CosmosPersonalityRepository), ServiceLifetime.Singleton));
    }
}