using MiroslavGPT.Domain.Factories;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MiroslavGPT.Domain
{
    public class TelegramMessageHandler: ITelegramMessageHandler
    {
        private readonly IBot _bot;
        private readonly ITelegramBotSettings _settings;
        private readonly ITelegramBotClient _telegramBotClient;

        public TelegramMessageHandler(IBot bot, ITelegramBotSettings telegramBotSettings, ITelegramClientFactory telegramClientFactory)
        {
            _bot = bot;
            _settings = telegramBotSettings;
            _telegramBotClient = telegramClientFactory.CreateBotClient(_settings.TelegramBotToken);
           
        }

        public async Task ProcessUpdateAsync(Update update)
        {
            if (update == null || update.Message == null || update.Message.Text == null || !update.Message.Text.StartsWith("/"))
            {
                return;
            }

            if (update.Message.Chat.Type != ChatType.Private && !update.Message.Text.Contains("@" + _settings.TelegramBotUsername))
            {
                return;
            }

            string text = update.Message.Text.Replace("@" + _settings.TelegramBotUsername, "").Trim();
            string response = await _bot.ProcessCommandAsync(update.Message.Chat.Id, update.Message.From.Username, text);
            await SendTextMessageAsync(update.Message.Chat.Id, response, update.Message.MessageId);
        }

        private async Task SendTextMessageAsync(long chatId, string response, int replyToMessageId)
        {
            await _telegramBotClient.SendTextMessageAsync(
                chatId: chatId,
                text: response,
                replyToMessageId: replyToMessageId,
                disableWebPagePreview: true
            );
        }
    }
}
