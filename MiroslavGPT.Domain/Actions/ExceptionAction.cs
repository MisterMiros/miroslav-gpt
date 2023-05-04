using Microsoft.Extensions.Logging;
using MiroslavGPT.Domain.Interfaces.Actions;
using MiroslavGPT.Domain.Interfaces.Clients;

namespace MiroslavGPT.Domain.Actions;

public class ExceptionAction: IExceptionAction
{
    private readonly ITelegramClient _telegramClient;
    private readonly ILogger<ExceptionAction> _logger;

    public ExceptionAction(ITelegramClient telegramClient, ILogger<ExceptionAction> logger)
    {
        _telegramClient = telegramClient;
        _logger = logger;
    }

    public async Task ExecuteAsync(long chatId, int messageId)
    {
        _logger.LogDebug("Executing exception action for chat {chatId} and message {messageId}", chatId, messageId);
        await _telegramClient.SendTextMessageAsync(chatId, "Something went wrong. Please try again later.", messageId);
    }
}