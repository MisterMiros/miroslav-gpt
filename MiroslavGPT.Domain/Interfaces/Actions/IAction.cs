using MiroslavGPT.Domain.Models.Commands;
using Telegram.Bot.Types;

namespace MiroslavGPT.Domain.Interfaces.Actions;

public interface IAction<TCommand> where TCommand : ICommand
{
    TCommand TryGetCommand(Update update);
    Task ExecuteAsync(TCommand command);
}