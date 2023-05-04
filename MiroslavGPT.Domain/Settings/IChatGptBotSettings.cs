namespace MiroslavGPT.Domain.Settings;

public interface IChatGptBotSettings
{
    public string SecretKey { get; set; }
    public string OpenAiApiKey { get; set; }
    public int MaxTokens { get; set; }
}