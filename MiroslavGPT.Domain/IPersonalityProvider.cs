namespace MiroslavGPT.Domain
{
    public interface IPersonalityProvider
    {
        List<OpenAI_API.Chat.ChatMessage> GetPersonalityMessages();
    }
}
