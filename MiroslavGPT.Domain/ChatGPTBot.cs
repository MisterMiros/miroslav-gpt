using MiroslavGPT.Domain.Extensions;
using MiroslavGPT.Domain.Factories;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Settings;
using OpenAI_API.Chat;
using Telegram.Bot.Types;

namespace MiroslavGPT.Domain
{
    public class ChatGPTBot: IBot
    {
        private readonly IChatGptBotSettings _settings;
        private readonly ITelegramBotSettings _telegramBotSettings;
        private readonly IUsersRepository _usersRepository;
        private readonly IPersonalityProvider _personalityProvider;
        private readonly IChatEndpoint _chatClient;
        private readonly IThreadRepository _threadRepository;

        public ChatGPTBot(
            IUsersRepository usersRepository, 
            IPersonalityProvider personalityProvider, 
            IChatGptBotSettings chatGptBotSettings,
            ITelegramBotSettings telegramBotSettings,
            IOpenAiClientFactory openAiClientFactory,
            IThreadRepository threadRepository)
        {
            _usersRepository = usersRepository;
            _personalityProvider = personalityProvider;
            _settings = chatGptBotSettings;
            _telegramBotSettings = telegramBotSettings;
            _chatClient = openAiClientFactory.CreateChatClient(_settings.OpenAiApiKey);
            _threadRepository = threadRepository;
        }

        public async Task<string> ProcessCommandAsync(Update update)
        {
            var chatId = update.Message.Chat.Id;
            var username = update.Message.From.Username;
            var messageId = update.Message.MessageId;
            var replyToId = update.Message.ReplyToMessage?.MessageId;
            var text = update.Message.Text?.Replace("@" + _telegramBotSettings.TelegramBotUsername, "").Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            var parts = text.Split(' ', 2);
            var command = parts[0];
            var argument = parts.Length > 1 ? parts[1].Trim() : string.Empty;

            if (command == "/init")
            {
                return await InitCommandAsync(chatId, argument);
            } 
            if (_personalityProvider.HasPersonalityCommand(command))
            {
                return await PromptCommandAsync(command, messageId, chatId, username, argument, replyToId);
            }

            return "Unknown command. Please use /init or /prompt.";
        }

        private async Task<string> InitCommandAsync(long chatId, string secretKey)
        {
            if (secretKey == _settings.SecretKey)
            {
                await _usersRepository.AuthorizeUserAsync(chatId);
                return "Authorization successful! You can now use /prompt command.";
            }
            else
            {
                return "Incorrect secret key. Please try again.";
            }
        }

        private async Task<string> PromptCommandAsync(string command, long messageId, long chatId, string username, string prompt, long? replyToId)
        {
            if (!await _usersRepository.IsAuthorizedAsync(chatId))
            {
                return "You are not authorized. Please use /init command with the correct secret key.";
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                return "Please provide a prompt after the /prompt command.";
            }

            var threadId = replyToId.HasValue
                ? await _threadRepository.GetThreadByMessage(chatId, replyToId.Value) ?? await _threadRepository.CreateThread(chatId)
                : await _threadRepository.CreateThread(chatId);

            await _threadRepository.AddThreadMessage(threadId, messageId, prompt, username);

            var threadMessages = await _threadRepository.GetMessages(threadId);
            var messages = _personalityProvider
                .GetPersonalityMessages(command)
                .Concat(threadMessages.Select(m => m.ToChatMessage()))
                .ToList();

            var response = await GetChatGPTResponse(prompt, messages);
            return response;
        }

        private async Task<string> GetChatGPTResponse(string prompt, List<ChatMessage> messages)
        {
            var request = new ChatRequest
            {
                Model = OpenAI_API.Models.Model.ChatGPTTurbo.ModelID,
                Messages = messages,
                MaxTokens = _settings.MaxTokens,
                Temperature = 0.7,
                TopP = 1,
                FrequencyPenalty = 0,
                PresencePenalty = 0
            };

            try
            {
                var result = await _chatClient.CreateChatCompletionAsync(request);
                var combinedResponse = string.Join("\n", result.Choices.Select(c => c.Message.Content.Trim()));
                return $"*Response from ChatGPT API for prompt '{prompt}':*\n\n{combinedResponse}";
            }
            catch (Exception e)
            {
                return $"Error while fetching response from ChatGPT API: {e.Message}";
            }
        }
    }
}