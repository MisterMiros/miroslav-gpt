using MiroslavGPT.Domain.Settings;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MiroslavGPT.Domain
{
    public class TelegramMessageHandler
    {
        private readonly ChatGPTBot _chatGPTBot;
        private readonly ITelegramBotSettings _settings;
        private readonly ITelegramBotClient _telegramBotClient;

        public TelegramMessageHandler(ChatGPTBot chatGPTBot, ITelegramBotSettings telegramBotSettings)
        {
            _chatGPTBot = chatGPTBot;
            _settings = telegramBotSettings;
            _telegramBotClient = new TelegramBotClient(_settings.TelegramBotToken);
           
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
            string response = await _chatGPTBot.ProcessCommandAsync(update.Message.Chat.Id, update.Message.From.Username, text);
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
