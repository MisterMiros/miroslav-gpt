namespace MiroslavGPT.Domain.Factories
{
    public interface IOpenAiClientFactory
    {
        public OpenAI_API.OpenAIAPI CreateClient(string openAiApiKey);
    }
}
