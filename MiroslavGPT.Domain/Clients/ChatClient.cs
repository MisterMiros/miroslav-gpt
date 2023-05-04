using MiroslavGPT.Domain.Interfaces.Clients;
using MiroslavGPT.Domain.Settings;
using OpenAI_API.Chat;

namespace MiroslavGPT.Domain.Clients;

public class ChatClient: IChatClient
{
    private readonly IChatGptBotSettings _chatGptBotSettings;
    private readonly IChatEndpoint _chatEndpoint;
    
    public ChatClient(IChatEndpoint chatEndpoint, IChatGptBotSettings chatGptBotSettings)
    { ;
        _chatEndpoint = chatEndpoint;
        _chatGptBotSettings = chatGptBotSettings;
    }
    
    public async Task<string> GetChatGptResponseAsync(string prompt, List<ChatMessage> messages)
    {
        var request = new ChatRequest
        {
            Model = OpenAI_API.Models.Model.ChatGPTTurbo.ModelID,
            Messages = messages,
            MaxTokens = _chatGptBotSettings.MaxTokens,
            Temperature = 0.7,
            TopP = 1,
            FrequencyPenalty = 0,
            PresencePenalty = 0
        };

        var result = await _chatEndpoint.CreateChatCompletionAsync(request);
        return string.Join("\n", result.Choices.Select(c => c.Message.Content.Trim()));
    }
}