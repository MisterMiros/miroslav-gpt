using Telegram.Bot.Types;

namespace MiroslavGPT.Domain.Interfaces.Clients;

public interface ITelegramClient
{
    Task<Message> SendTextMessageAsync(long chatId, string response, int? replyToMessageId = null);
}