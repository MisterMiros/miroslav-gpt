using Microsoft.Extensions.DependencyInjection;
using MiroslavGPT.Domain.Actions;
using MiroslavGPT.Domain.Clients;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Interfaces.Actions;
using MiroslavGPT.Domain.Interfaces.Clients;
using MiroslavGPT.Domain.Interfaces.Personality;
using MiroslavGPT.Domain.Models.Commands;
using MiroslavGPT.Domain.Personality;
using MiroslavGPT.Domain.Settings;
using OpenAI_API;
using OpenAI_API.Chat;
using Telegram.Bot;

namespace MiroslavGPT.Domain.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddSingleton<IChatEndpoint>(s => new OpenAIAPI(s.GetService<IChatGptBotSettings>()!.OpenAiApiKey).Chat);
        services.AddSingleton<ITelegramBotClient>(s => new TelegramBotClient(s.GetService<ITelegramBotSettings>()!.TelegramBotToken));
            
        services.AddSingleton<ITelegramClient, TelegramClient>();
        services.AddSingleton<IChatClient, ChatClient>();
            
        services.AddSingleton<IPersonalityProvider, PersonalityProvider>();
            
        services.AddSingleton<IAction<InitCommand>, InitAction>();
        services.AddSingleton<IAction<PromptCommand>, PromptAction>();
        services.AddSingleton<IAction<UnknownCommand>, UnknownAction>();
        services.AddSingleton<IExceptionAction, ExceptionAction>();
            
        services.AddSingleton<ITelegramMessageHandler, TelegramMessageHandler>();
            
        return services;
    }
}