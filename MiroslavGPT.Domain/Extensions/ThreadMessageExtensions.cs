using MiroslavGPT.Domain.Models;
using MiroslavGPT.Domain.Models.Threads;
using OpenAI_API.Chat;

namespace MiroslavGPT.Domain.Extensions
{
    public static class ThreadMessageExtensions
    {
        public static ChatMessage ToChatMessage(this ThreadMessage message)
        {
            return new ChatMessage
            {
                Role = message.IsAssistant ? ChatMessageRole.Assistant : ChatMessageRole.User,
                Content = string.IsNullOrWhiteSpace(message.Username) ? message.Text : $"@{message.Username}: {message.Text}"
            };
        }
    }
}
