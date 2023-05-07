using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Settings;
using System.Collections.Immutable;
using MiroslavGPT.Domain.Interfaces.Actions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Logging;

namespace MiroslavGPT.Domain;

public class TelegramMessageHandler : ITelegramMessageHandler
{
    private readonly ImmutableArray<ChatType> _allowedTypes = new[]
    {
        ChatType.Group,
        ChatType.Private,
    }.ToImmutableArray();

    private readonly IEnumerable<IAction> _actions;
    private readonly IExceptionAction _exceptionAction;
    private readonly ITelegramBotSettings _settings;
    private readonly ILogger<TelegramMessageHandler> _logger;

    public TelegramMessageHandler(IEnumerable<IAction> actions, IExceptionAction exceptionAction, ITelegramBotSettings settings, ILogger<TelegramMessageHandler> logger)
    {
        _actions = actions;
        _exceptionAction = exceptionAction;
        _settings = settings;
        _logger = logger;
    }

    public async Task ProcessUpdateAsync(Update? update)
    {
        if (update?.Message == null || string.IsNullOrWhiteSpace(update.Message.Text) || !update.Message.Text.StartsWith("/"))
        {
            _logger.LogDebug("Update is not a command");
            return;
        }

        if (!_allowedTypes.Contains(update.Message.Chat.Type))
        {
            _logger.LogDebug("Update is not from supported chat type");
            return;
        }

        if (update.Message.Chat.Type != ChatType.Private && !update.Message.Text.Contains("@" + _settings.TelegramBotUsername))
        {
            _logger.LogDebug("Update is not in private chat or is not a bot command");
            return;
        }

        try
        {
            _logger.LogDebug("Choosing the right action for the update");
            foreach (var action in _actions)
            {
                _logger.LogDebug("Checking action {ActionName}", action.GetType().Name);
                var command = action.TryGetCommand(update);
                if (command != null)
                {
                    _logger.LogDebug("Executing action {ActionName}", action.GetType().Name);
                    await action.ExecuteAsync(command);
                    return;
                }
            }
            _logger.LogDebug("No action found for processing");
        }
        catch (Exception)
        {
            await _exceptionAction.ExecuteAsync(update.Message.Chat.Id, update.Message.MessageId);
            throw;
        }
    }
}