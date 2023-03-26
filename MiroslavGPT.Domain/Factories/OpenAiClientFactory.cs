using OpenAI_API;
using OpenAI_API.Chat;

namespace MiroslavGPT.Domain.Factories
{
    public class OpenAiClientFactory : IOpenAiClientFactory
    {
        public IChatEndpoint CreateChatClient(string openAiApiKey)
        {
            return new OpenAIAPI(openAiApiKey).Chat;
        }
    }
}
