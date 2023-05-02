using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Interfaces.Actions;
using MiroslavGPT.Domain.Interfaces.Clients;
using MiroslavGPT.Domain.Models.Commands;
using Telegram.Bot.Types;

namespace MiroslavGPT.Domain.Actions;

public abstract class BaseAction<TCommand> : IAction<TCommand> where TCommand : ICommand
{
    protected readonly ITelegramClient _telegramClient;

    protected BaseAction(ITelegramClient telegramClient)
    {
        _telegramClient = telegramClient;
    }

    public abstract TCommand TryGetCommand(Update update);

    public abstract Task ExecuteAsync(TCommand command);
}