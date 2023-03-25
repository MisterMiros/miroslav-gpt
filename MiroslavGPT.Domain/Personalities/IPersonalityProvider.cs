namespace MiroslavGPT.Domain.Personalities
{
    public interface IPersonalityProvider
    {
        List<OpenAI_API.Chat.ChatMessage> GetPersonalityMessages();
    }
}
