namespace MiroslavGPT.Domain.Models
{
    public class BotResponse
    {
        public string Text { get; set; }
        public MemoryStream Sound { get; set; }

        public static BotResponse From(string text)
        {
            return new BotResponse
            {
                Text = text
            };
        }

        public static BotResponse From(string text, MemoryStream sound)
        {
            return new BotResponse
            {
                Text = text,
                Sound = sound,
            };
        }
    }
}
