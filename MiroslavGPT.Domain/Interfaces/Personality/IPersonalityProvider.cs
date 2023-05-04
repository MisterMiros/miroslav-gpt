using OpenAI_API.Chat;

namespace MiroslavGPT.Domain.Interfaces.Personality;

public interface IPersonalityProvider
{
    bool HasPersonalityCommand(string command);
    List<ChatMessage> GetPersonalityMessages(string command);
}