using MiroslavGPT.Domain.Factories;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Personalities;
using MiroslavGPT.Domain.Settings;
using OpenAI_API;

namespace MiroslavGPT.Domain
{
    public class ChatGPTBot: IBot
    {
        private readonly IChatGptBotSettings _settings;
        private readonly IUsersRepository _usersRepository;
        private readonly IPersonalityProvider _personalityProvider;
        private readonly OpenAIAPI _openAIApi;

        public ChatGPTBot(IUsersRepository usersRepository, IPersonalityProvider personalityProvider, IChatGptBotSettings chatGptBotSettings, IOpenAiClientFactory openAiClientFactory)
        {
            _usersRepository = usersRepository;
            _personalityProvider = personalityProvider;
            _settings = chatGptBotSettings;
            _openAIApi = openAiClientFactory.CreateClient(_settings.OpenAiApiKey);
        }

        public async Task<string> ProcessCommandAsync(long chatId, string username, string text)
        {
            string[] parts = text.Split(' ', 2);
            string command = parts[0];
            string argument = parts.Length > 1 ? parts[1] : string.Empty;

            switch (command)
            {
                case "/init":
                    return await InitCommandAsync(chatId, argument);
                case "/prompt":
                    return await PromptCommandAsync(chatId, username, argument);
                default:
                    return "Unknown command. Please use /init or /prompt.";
            }
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

        private async Task<string> PromptCommandAsync(long chatId, string username, string prompt)
        {
            if (!await _usersRepository.IsAuthorizedAsync(chatId))
            {
                return "You are not authorized. Please use /init command with the correct secret key.";
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                return "Please provide a prompt after the /prompt command.";
            }

            string response = await GetChatGPTResponse(username, prompt); // Implement this method to call ChatGPT API
            return response;
        }

        private async Task<string> GetChatGPTResponse(string username, string prompt)
        {
            var messages = _personalityProvider.GetPersonalityMessages();
            messages.Add(new OpenAI_API.Chat.ChatMessage
            {
                Role = OpenAI_API.Chat.ChatMessageRole.User,
                Content = $"@{username}: {prompt}",
            });
            var request = new OpenAI_API.Chat.ChatRequest
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