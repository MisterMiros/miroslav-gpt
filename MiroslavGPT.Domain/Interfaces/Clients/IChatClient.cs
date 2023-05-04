using OpenAI_API.Chat;

namespace MiroslavGPT.Domain.Interfaces.Clients;

public interface IChatClient
{
    Task<string> GetChatGptResponseAsync(string prompt, List<ChatMessage> messages);
}