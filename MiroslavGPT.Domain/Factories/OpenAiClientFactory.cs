using OpenAI_API;

namespace MiroslavGPT.Domain.Factories
{
    public class OpenAiClientFactory : IOpenAiClientFactory
    {
        public OpenAIAPI CreateClient(string openAiApiKey)
        {
            return new OpenAIAPI(openAiApiKey);
        }
    }
}
