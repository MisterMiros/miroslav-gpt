using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Settings;
using System.Collections.Immutable;
using MiroslavGPT.Domain.Interfaces.Actions;
using MiroslavGPT.Domain.Models.Commands;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MiroslavGPT.Domain;

public class TelegramMessageHandler : ITelegramMessageHandler
{
    private readonly ImmutableArray<ChatType> _allowedTypes = new[]
    {
        ChatType.Group,
        ChatType.Private,
    }.ToImmutableArray();

    private readonly IEnumerable<IAction<ICommand>> _actions;
    private readonly IExceptionAction _exceptionAction;
    private readonly ITelegramBotSettings _settings;

    public TelegramMessageHandler(IEnumerable<IAction<ICommand>> actions, IExceptionAction exceptionAction, ITelegramBotSettings settings)
    {
        _actions = actions;
        _exceptionAction = exceptionAction;
        _settings = settings;
    }

    public async Task ProcessUpdateAsync(Update update)
    {
        if (update?.Message == null || string.IsNullOrWhiteSpace(update.Message.Text) || !update.Message.Text.StartsWith("/"))
        {
            return;
        }

        if (!_allowedTypes.Contains(update.Message.Chat.Type))
        {
            return;
        }

        if (update.Message.Chat.Type != ChatType.Private && !update.Message.Text.Contains("@" + _settings.TelegramBotUsername))
        {
            return;
        }

        try
        {
            foreach (var action in _actions)
            {
                var command = action.TryGetCommand(update);
                if (command != null)
                {
                    await action.ExecuteAsync(command);
                    return;
                }
            }
        }
        catch (Exception)
        {
            await _exceptionAction.ExecuteAsync(update.Message.Chat.Id, update.Message.MessageId);
            throw;
        }
    }
}