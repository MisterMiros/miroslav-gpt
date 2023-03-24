using System;
using System.Threading.Tasks;
using OpenAI_API;
using Telegram.Bot.Types.Enums;

namespace MiroslavGPT.Domain
{
    public class ChatGPTBot
    {
        private readonly string _secretKey;
        private readonly IUsersRepository _usersRepository;
        private readonly IPersonalityProvider _personalityProvider;
        private readonly OpenAIAPI _openAIApi;
        private readonly int _maxTokens;

        public ChatGPTBot(string secretKey, IUsersRepository usersRepository, IPersonalityProvider personalityProvider, string openAiApiKey, int maxTokens)
        {
            _secretKey = secretKey;
            _usersRepository = usersRepository;
            _personalityProvider = personalityProvider;
            _openAIApi = new OpenAIAPI(openAiApiKey);
            _maxTokens = maxTokens;
        }

        public async Task<string> ProcessCommandAsync(long chatId, string text)
        {
            string[] parts = text.Split(' ', 2);
            string command = parts[0];
            string argument = parts.Length > 1 ? parts[1] : string.Empty;

            switch (command)
            {
                case "/init":
                    return await InitCommandAsync(chatId, argument);
                case "/prompt":
                    return await PromptCommandAsync(chatId, argument);
                default:
                    return "Unknown command. Please use /init or /prompt.";
            }
        }

        private async Task<string> InitCommandAsync(long chatId, string secretKey)
        {
            if (secretKey == _secretKey)
            {
                await _usersRepository.AuthorizeUserAsync(chatId);
                return "Authorization successful! You can now use /prompt command.";
            }
            else
            {
                return "Incorrect secret key. Please try again.";
            }
        }

        private async Task<string> PromptCommandAsync(long chatId, string prompt)
        {
            if (!await _usersRepository.IsAuthorizedAsync(chatId))
            {
                return "You are not authorized. Please use /init command with the correct secret key.";
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                return "Please provide a prompt after the /prompt command.";
            }

            string response = await GetChatGPTResponse(prompt); // Implement this method to call ChatGPT API
            return response;
        }

        private async Task<string> GetChatGPTResponse(string prompt)
        {
            var request = new OpenAI_API.Chat.ChatRequest
            {
                Model = OpenAI_API.Models.Model.ChatGPTTurbo.ModelID,
                Messages = new List<OpenAI_API.Chat.ChatMessage>
                {
                    new OpenAI_API.Chat.ChatMessage
                    {
                        Role = OpenAI_API.Chat.ChatMessageRole.System,
                        Content = _personalityProvider.GetSystemMessage(),
                    },
                    new OpenAI_API.Chat.ChatMessage
                    {
                        Role = OpenAI_API.Chat.ChatMessageRole.System,
                        Content = prompt,
                    }

                },
                MaxTokens = _maxTokens,
                Temperature = 0.7,
                TopP = 1,
                FrequencyPenalty = 0,
                PresencePenalty = 0
            };

            try
            {
                var result = await _openAIApi.Chat.CreateChatCompletionAsync(request);
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