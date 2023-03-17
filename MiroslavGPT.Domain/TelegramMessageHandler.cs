using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MiroslavGPT.Domain
{
    public class TelegramMessageHandler
    {
        private readonly ChatGPTBot _chatGPTBot;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly string _botUsername;

        public TelegramMessageHandler(ChatGPTBot chatGPTBot, string telegramBotToken, string botUsername)
        {
            _chatGPTBot = chatGPTBot;
            _telegramBotClient = new TelegramBotClient(telegramBotToken);
            _botUsername = botUsername;
        }

        public async Task ProcessUpdateAsync(Update update)
        {
            if (update == null || update.Message == null || update.Message.Text == null || !update.Message.Text.StartsWith("/"))
            {
                return;
            }

            if (update.Message.Chat.Type != ChatType.Private && !update.Message.Text.Contains("@" + _botUsername))
            {
                return;
            }

            string text = update.Message.Text.Replace("@" + _botUsername, "").Trim();
            string response = await _chatGPTBot.ProcessCommandAsync(update.Message.Chat.Id, text);
            await SendTextMessageAsync(update.Message.Chat.Id, response, update.Message.MessageId);
        }

        private async Task SendTextMessageAsync(long chatId, string response, int replyToMessageId)
        {
            await _telegramBotClient.SendTextMessageAsync(
                chatId: chatId,
                text: response,
                parseMode: ParseMode.Markdown,
                replyToMessageId: replyToMessageId,
                disableWebPagePreview: true
            );
        }
    }
}
