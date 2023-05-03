using MiroslavGPT.Domain.Interfaces.Clients;
using MiroslavGPT.Domain.Models.Commands;
using Telegram.Bot.Types;

namespace MiroslavGPT.Domain.Actions;

public class UnknownAction : BaseAction
{
    public UnknownAction(ITelegramClient telegramClient) : base(telegramClient)
    {
    }

    public override ICommand TryGetCommand(Update update)
    {
        return new UnknownCommand
        {
            ChatId = update.Message!.Chat.Id,
            MessageId = update.Message.MessageId,
        };
    }

    public override async Task ExecuteAsync(ICommand command)
    {
        await TelegramClient.SendTextMessageAsync(command.ChatId, "Unknown command. Please use /init or one of the personality commands.", command.MessageId);
    }
}