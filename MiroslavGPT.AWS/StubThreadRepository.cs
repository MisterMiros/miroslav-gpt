﻿using System.Collections.Concurrent;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Interfaces.Threads;
using MiroslavGPT.Domain.Models;

namespace MiroslavGPT.AWS;

public class StubThreadRepository: IThreadRepository
{
    private ConcurrentDictionary<Guid, ThreadMessage> Threads { get; set; }
    
    public StubThreadRepository()
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

    public Task AddThreadMessageAsync(Guid id, long messageId, string text, string username)
    {
        var message = Threads[id];
        message.MessageId = messageId;
        message.Username = username;
        message.Text = text;
        message.IsAssistant = false;
        return Task.CompletedTask;
    }

    public Task<List<ThreadMessage>> GetMessagesAsync(Guid id)
    {
        return Task.FromResult(new List<ThreadMessage> { Threads[id] });
    }
}