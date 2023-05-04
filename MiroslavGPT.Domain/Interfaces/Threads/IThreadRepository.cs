using MiroslavGPT.Domain.Models.Threads;

namespace MiroslavGPT.Domain.Interfaces.Threads;

public interface IThreadRepository
{
    public Task<MessageThread> CreateThreadAsync(long chatId);
    public Task<MessageThread?> GetThreadByMessageAsync(long chatId, int messageId);
    public Task UpdateThreadAsync(MessageThread messageThread);
}