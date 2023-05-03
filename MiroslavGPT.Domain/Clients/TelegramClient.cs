using Microsoft.Extensions.Logging;
using MiroslavGPT.Domain.Interfaces.Clients;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MiroslavGPT.Domain.Clients;

public class TelegramClient: ITelegramClient
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ILogger<TelegramClient> _logger;

    public TelegramClient(ITelegramBotClient telegramBotClient, ILogger<TelegramClient> logger)
    {
        _telegramBotClient = telegramBotClient;
        _logger = logger;
    }
    
    public async Task<Message> SendTextMessageAsync(long chatId, string response, int? replyToMessageId = null)
    {
        _logger.LogDebug("Sending message to chat {chatId}: {response}", chatId, response);
        return await _telegramBotClient.SendTextMessageAsync(
            chatId: chatId,
            text: response,
            replyToMessageId: replyToMessageId,
            parseMode: ParseMode.Markdown,
            disableWebPagePreview: true
        );
    }
}