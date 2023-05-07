using MiroslavGPT.Domain.Models.Commands;
using Telegram.Bot.Types;

namespace MiroslavGPT.Domain.Interfaces.Actions;

public interface IAction
{
    ICommand? TryGetCommand(Update update);
    Task ExecuteAsync(ICommand abstractCommand);
}