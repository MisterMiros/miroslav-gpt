using OpenAI_API.Chat;

namespace MiroslavGPT.Domain.Factories
{
    public interface IOpenAiClientFactory
    {
        public IChatEndpoint CreateChatClient(string openAiApiKey);
    }
}
