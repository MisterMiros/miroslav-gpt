using MiroslavGPT.Domain.Models;

namespace MiroslavGPT.Domain.Interfaces.Threads
{
    public interface IThreadRepository
    {
        public Task<Guid> CreateThreadAsync(long chatId);
        public Task<Guid?> GetThreadByMessageAsync(long chatId, long messageId);
        public Task AddThreadMessageAsync(Guid id, long messageId, string text, string username);
        public Task<List<ThreadMessage>> GetMessagesAsync(Guid id);
    }
}
