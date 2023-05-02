namespace MiroslavGPT.Domain.Models.Threads
{
    public record ThreadMessage
    {
        public long MessageId { get; set; }
        public string Username { get; set; }
        public string Text { get; set; }
        public bool IsAssistant { get; set; }
    }
}
