using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using MiroslavGPT.Azure.Settings;
using MiroslavGPT.Domain.Interfaces.Threads;
using MiroslavGPT.Domain.Models.Threads;

namespace MiroslavGPT.Azure.Threads;

public class CosmosThreadRepository : IThreadRepository
{
    private readonly CosmosClient _client;
    private readonly IThreadSettings _settings;
    private readonly Container _threadsContainer;

    public CosmosThreadRepository(CosmosClient client, IThreadSettings settings)
    {
        _client = client;
        _settings = settings;
        _threadsContainer = _client.GetContainer(settings.ThreadDatabaseName, settings.ThreadContainerName);
    }

    public async Task<MessageThread> CreateThreadAsync(long chatId)
    {
        var thread = new MessageThread
        {
            ChatId = chatId,
            Id = Guid.NewGuid(),
            Messages = new List<ThreadMessage>(),
        };
        await _threadsContainer.CreateItemAsync(thread, new PartitionKey(thread.Id.ToString()));
        return thread;
    }

    public async Task<MessageThread> GetThreadByMessageAsync(long chatId, int messageId)
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.chatId = @chatId AND ARRAY_CONTAINS(c.messages, { messageId: @messageId }, true)")
                .WithParameter("@chatId", chatId)
                .WithParameter("@messageId", messageId);
            var iterator = _threadsContainer.GetItemQueryIterator<MessageThread>(query);
            if (!iterator.HasMoreResults)
            {
                return null;
            }

            var response = await iterator.ReadNextAsync();
            return response.FirstOrDefault();
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task UpdateThreadAsync(MessageThread messageThread)
    {
        messageThread.Messages = messageThread.Messages.TakeLast(_settings.ThreadLengthLimit).ToList();
        await _threadsContainer.ReplaceItemAsync(messageThread, messageThread.Id.ToString(), new PartitionKey(messageThread.Id.ToString()));
    }
}