using Microsoft.Extensions.DependencyInjection;
using MiroslavGPT.Domain.Factories;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Personalities;

namespace MiroslavGPT.Domain.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddSingleton<ITelegramClientFactory, TelegramClientFactory>();
            services.AddSingleton<IOpenAiClientFactory, OpenAiClientFactory>();
            services.AddSingleton<IPersonalityProvider, PersonalityProvider>();
            services.AddSingleton<IBot, ChatGPTBot>();
            services.AddSingleton<ITelegramMessageHandler, TelegramMessageHandler>();
            return services;
        }
    }
}
