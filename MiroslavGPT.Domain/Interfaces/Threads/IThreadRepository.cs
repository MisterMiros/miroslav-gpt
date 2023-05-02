using MiroslavGPT.Domain.Models;
using MiroslavGPT.Domain.Models.Threads;
using Thread = MiroslavGPT.Domain.Models.Threads.Thread;

namespace MiroslavGPT.Domain.Interfaces.Threads
{
    public interface IThreadRepository
    {
        public Task<Thread> CreateThreadAsync(long chatId);
        public Task<Thread?> GetThreadByMessageAsync(long chatId, long messageId);
        public Task UpdateThreadAsync(Thread thread);
    }
}
