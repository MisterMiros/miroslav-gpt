using MiroslavGPT.Domain.Models;

namespace MiroslavGPT.Domain.Interfaces
{
    public interface IThreadRepository
    {
        public Task<Guid> CreateThread(long chatId);
        public Task<Guid> GetThreadByMessage(long chatId, long messageId);
        public Task AddThreadMessage(Guid id, long messageId, string message, long? userId = null);
        public Task<List<ThreadMessage>> GetMessages(Guid id);
    }
}
