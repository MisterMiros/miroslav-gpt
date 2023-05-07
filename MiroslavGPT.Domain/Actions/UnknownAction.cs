using Microsoft.Extensions.Logging;
using MiroslavGPT.Domain.Interfaces.Clients;
using MiroslavGPT.Domain.Models.Commands;
using Telegram.Bot.Types;

namespace MiroslavGPT.Domain.Actions;

public class UnknownAction : BaseAction
{
    private readonly ILogger<UnknownAction> _logger;

    public UnknownAction(ITelegramClient telegramClient, ILogger<UnknownAction> logger) : base(telegramClient)
    {
        _logger = logger;
    }

    public override ICommand TryGetCommand(Update update)
    {
        _logger.LogDebug("Defaulting to unknown command");
        return new UnknownCommand
        {
            ChatId = update.Message!.Chat.Id,
            MessageId = update.Message.MessageId,
        };
    }

    public override async Task ExecuteAsync(ICommand command)
    {
        _logger.LogDebug("Sending unknown command message to user {ChatId}", command.ChatId);
        await TelegramClient.SendTextMessageAsync(command.ChatId, "Unknown command. Please use /init or one of the personality commands.", command.MessageId);
    }
}