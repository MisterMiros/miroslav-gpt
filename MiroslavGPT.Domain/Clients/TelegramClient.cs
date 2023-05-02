﻿using MiroslavGPT.Domain.Interfaces.Clients;
using MiroslavGPT.Domain.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MiroslavGPT.Domain.Clients;

public class TelegramClient: ITelegramClient
{
    private readonly ITelegramBotClient _telegramBotClient;
    public TelegramClient(ITelegramBotSettings telegramBotSettings)
    {
        _telegramBotClient = new TelegramBotClient(telegramBotSettings.TelegramBotToken);
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