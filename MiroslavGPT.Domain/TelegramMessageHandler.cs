using MiroslavGPT.Domain.Factories;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Settings;
using System.Collections.Immutable;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MiroslavGPT.Domain
{
    public class TelegramMessageHandler : ITelegramMessageHandler
    {
        private readonly ImmutableArray<ChatType> _allowedTypes = new[]
        {
            ChatType.Group,
            ChatType.Private,
        }.ToImmutableArray();

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
            if (update == null || update.Message == null || string.IsNullOrWhiteSpace(update.Message.Text) || !update.Message.Text.StartsWith("/"))
            {
                return;
            }

            if (!_allowedTypes.Contains(update.Message.Chat.Type))
            {
                return;
            }

            if (update.Message.Chat.Type != ChatType.Private && !update.Message.Text.Contains("@" + _settings.TelegramBotUsername))
            {
                return;
            }

            try
            {
                var response = await _bot.ProcessCommandAsync(update.Message.Chat.Id, update.Message.From.Username, text);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    await SendTextMessageAsync(update.Message.Chat.Id, response, update.Message.MessageId);
                }
            }
            catch (Exception)
            {
                await SendTextMessageAsync(update.Message.Chat.Id, "Error handling the command", update.Message.MessageId);
                throw;
            }
        }

        private async Task SendTextMessageAsync(long chatId, string response, int replyToMessageId)
        {
            await _telegramBotClient.SendTextMessageAsync(
                chatId: chatId,
                text: response,
                replyToMessageId: replyToMessageId,
                parseMode: ParseMode.Markdown,
                disableWebPagePreview: true
            );
        }
    }
}
