using OpenAI_API.Chat;

namespace MiroslavGPT.Domain.Interfaces
{
    public interface IPersonalityProvider
    {
        bool HasPersonalityCommand(string command);
        List<ChatMessage> GetPersonalityMessages(string command);
    }
}
