using MiroslavGPT.Domain.Interfaces.Actions;
using MiroslavGPT.Domain.Interfaces.Clients;
using MiroslavGPT.Domain.Models.Commands;
using Telegram.Bot.Types;

namespace MiroslavGPT.Domain.Actions;

public abstract class BaseAction: IAction
{
    protected readonly ITelegramClient TelegramClient;

    protected BaseAction(ITelegramClient telegramClient)
    {
        TelegramClient = telegramClient;
    }

    public abstract ICommand TryGetCommand(Update update);

    public abstract Task ExecuteAsync(ICommand abstractCommand);
}