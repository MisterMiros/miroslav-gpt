using System.Collections.Concurrent;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Interfaces.Threads;
using MiroslavGPT.Domain.Models;

namespace MiroslavGPT.AWS;

public class StubThreadsRepository: IThreadsRepository
{
    private ConcurrentDictionary<Guid, ThreadMessage> Threads { get; set; }
    
    public StubThreadsRepository()
    {
        Threads = new ConcurrentDictionary<Guid, ThreadMessage>();
    }


    public Task<Guid> CreateThreadAsync(long chatId)
    {
        var id = Guid.NewGuid();
        Threads[id] = new ThreadMessage();
        return Task.FromResult(id);
    }

    public Task<Guid?> GetThreadByMessageAsync(long chatId, long messageId)
    {
        return Task.FromResult((Guid?)null);
    }

    public Task AddThreadMessageAsync(Guid id, long messageId, string text, string username, bool isAssistant)
    {
        var message = Threads[id];
        message.MessageId = messageId;
        message.Username = username;
        message.Text = text;
        message.IsAssistant = isAssistant;
        return Task.CompletedTask;
    }
    
    public Task<List<ThreadMessage>> GetMessagesAsync(Guid id)
    {
        return Task.FromResult(new List<ThreadMessage> { Threads[id] });
    }
}