using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using MiroslavGPT.Azure.Settings;
using MiroslavGPT.Domain.Interfaces.Threads;
using MiroslavGPT.Domain.Models;
using MiroslavGPT.Domain.Models.Threads;

namespace MiroslavGPT.Azure.Threads;

public class CosmosThreadRepository : IThreadRepository
{
    private readonly CosmosClient _client;
    private readonly ICosmosThreadSettings _settings;
    private readonly Container _threadsContainer;

    public CosmosThreadRepository(CosmosClient client, ICosmosThreadSettings settings)
    {
        _client = client;
        _settings = settings;
        _threadsContainer = _client.GetContainer(settings.ThreadDatabaseName, settings.ThreadContainerName);
    }

    public async Task<Thread> CreateThreadAsync(long chatId)
    {
        var thread = new Thread
        {
            ChatId = chatId,
            Id = Guid.NewGuid(),
            Messages = new List<ThreadMessage>(),
        };
        await _threadsContainer.CreateItemAsync(thread, new PartitionKey(thread.Id.ToString()));
        return thread;
    }

    public async Task<Thread?> GetThreadByMessageAsync(long chatId, long messageId)
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.chatId = @chatId AND ARRAY_CONTAINS(c.messages, { messageId: @messageId }, true)")
                .WithParameter("@chatId", chatId)
                .WithParameter("@messageId", messageId);
            var iterator = _threadsContainer.GetItemQueryIterator<Thread>(query);
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

    public async Task UpdateThreadAsync(Thread thread)
    {
        thread.Messages = thread.Messages.TakeLast(_settings.ThreadLengthLimit).ToList();
        await _threadsContainer.ReplaceItemAsync(thread, thread.Id.ToString(), new PartitionKey(thread.Id.ToString()));
    }
}