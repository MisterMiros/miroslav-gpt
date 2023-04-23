namespace MiroslavGPT.Domain.Models
{
    public class ThreadMessage
    {
        public long? MessageId { get; set; }
        public long? UserId { get; set; }
        public string Message { get; set; }
    }
}
