using MiroslavGPT.Domain.Factories;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Models;
using MiroslavGPT.Domain.Settings;
using OpenAI_API.Chat;

namespace MiroslavGPT.Domain
{
    public class ChatGPTBot: IBot
    {
        private readonly IChatGptBotSettings _settings;
        private readonly IVoiceOverService _voiceOverService;
        private readonly IUsersRepository _usersRepository;
        private readonly IPersonalityProvider _personalityProvider;
        private readonly IChatEndpoint _chatClient;

        public ChatGPTBot(IUsersRepository usersRepository, IPersonalityProvider personalityProvider, IChatGptBotSettings chatGptBotSettings, IOpenAiClientFactory openAiClientFactory, IVoiceOverService voiceOverService)
        {
            _usersRepository = usersRepository;
            _personalityProvider = personalityProvider;
            _settings = chatGptBotSettings;
            _voiceOverService = voiceOverService;
            _chatClient = openAiClientFactory.CreateChatClient(_settings.OpenAiApiKey);
        }

        public async Task<BotResponse> ProcessCommandAsync(long chatId, string username, string text)
        {
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
            if (command == "/enablevoice")
            {
                return await EnableVoiceOverCommandAsync(chatId);
            }
            if (command == "/disablevoice")
            {
                return await DisableVoiceOverCommandAsync(chatId);
            }
            if (_personalityProvider.HasPersonalityCommand(command))
            {
                return await PromptCommandAsync(command, chatId, username, argument);
            }

            return BotResponse.From("Unknown command.");
        }

        private async Task<BotResponse> EnableVoiceOverCommandAsync(long chatId)
        {
            if (!await _usersRepository.IsAuthorizedAsync(chatId))
            {
                return BotResponse.From("You are not authorized. Please use /init command with the correct secret key.");
            }
            await _usersRepository.SetVoiceOverAsync(chatId, true);
            return BotResponse.From("Successfully enabled voice over for responses.");
        }

        private async Task<BotResponse> DisableVoiceOverCommandAsync(long chatId)
        {
            if (!await _usersRepository.IsAuthorizedAsync(chatId))
            {
                return BotResponse.From("You are not authorized. Please use /init command with the correct secret key.");
            }
            await _usersRepository.SetVoiceOverAsync(chatId, false);
            return BotResponse.From("Successfully disabled voice over for responses.");
        }

        private async Task<BotResponse> InitCommandAsync(long chatId, string secretKey)
        {
            if (secretKey == _settings.SecretKey)
            {
                await _usersRepository.AuthorizeUserAsync(chatId);
                return BotResponse.From("Authorization successful! You can now use /prompt command.");
            }
            else
            {
                return BotResponse.From("Incorrect secret key. Please try again.");
            }
        }

        private async Task<BotResponse> PromptCommandAsync(string command, long chatId, string username, string prompt)
        {
            if (!await _usersRepository.IsAuthorizedAsync(chatId))
            {
                return BotResponse.From("You are not authorized. Please use /init command with the correct secret key.");
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                return BotResponse.From("Please provide a prompt after the /prompt command.");
            }

            var chatResponse = await GetChatGPTResponse(command, username, prompt);
            var textRespone = $"*Response from ChatGPT API for prompt '{prompt}':*\n\n{chatResponse}";

            if (!await _usersRepository.IsVoiceOverEnabledAsync(chatId))
            {
                return BotResponse.From(textRespone);
            }

            var soundResponse = _voiceOverService.VoiceOver(chatResponse);
            return BotResponse.From(textRespone, soundResponse);
        }

        private async Task<string> GetChatGPTResponse(string command, string username, string prompt)
        {
            var messages = _personalityProvider.GetPersonalityMessages(command).Append(new ChatMessage
            {
                Role = ChatMessageRole.User,
                Content = string.IsNullOrWhiteSpace(username) ? prompt : $"@{username}: {prompt}",
            }).ToList();
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

            var result = await _chatClient.CreateChatCompletionAsync(request);
            return string.Join("\n", result.Choices.Select(c => c.Message.Content.Trim()));
        }
    }
}