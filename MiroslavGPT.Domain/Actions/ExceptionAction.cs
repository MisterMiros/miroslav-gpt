using MiroslavGPT.Domain.Interfaces.Actions;
using MiroslavGPT.Domain.Interfaces.Clients;

namespace MiroslavGPT.Domain.Actions;

public class ExceptionAction: IExceptionAction
{
    private readonly ITelegramClient _telegramClient;

    public ExceptionAction(ITelegramClient telegramClient)
    {
        _telegramClient = telegramClient;
    }

    public async Task ExecuteAsync(long chatId, int messageId)
    {
        await _telegramClient.SendTextMessageAsync(chatId, "Something went wrong. Please try again later.", messageId);
    }
}