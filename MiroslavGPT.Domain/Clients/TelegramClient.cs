using Microsoft.Extensions.Logging;
using MiroslavGPT.Domain.Interfaces.Clients;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MiroslavGPT.Domain.Clients;

public class TelegramClient : ITelegramClient
{
    private readonly ITelegramBotClient _telegramBotClient;

    public TelegramClient(ITelegramBotClient telegramBotClient)
    {
        _telegramBotClient = telegramBotClient;
    }

    public async Task<Message> SendTextMessageAsync(long chatId, string response, int? replyToMessageId = null)
    {
        return await _telegramBotClient.SendTextMessageAsync(
            chatId: chatId,
            text: response,
            replyToMessageId: replyToMessageId,
            parseMode: ParseMode.Markdown,
            disableWebPagePreview: true
        );
    }
}