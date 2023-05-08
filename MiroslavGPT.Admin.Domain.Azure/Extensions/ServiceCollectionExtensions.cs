using System.Reflection.Emit;
using Microsoft.Extensions.DependencyInjection;
using MiroslavGPT.Admin.Domain.Azure.Personalities;
using MiroslavGPT.Admin.Domain.Interfaces.Personalities;

namespace MiroslavGPT.Admin.Domain.Azure.Extensions;

public static class ServiceCollectionExtensions
{
    public static ServiceCollection AddAzureAdminDomainServices(this ServiceCollection services)
    {
        services.AddSingleton<IPersonalityRepository, CosmosPersonalityRepository>();
        return services;
    } 
}