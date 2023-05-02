using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using MiroslavGPT.Azure.Settings;
using MiroslavGPT.Domain.Interfaces.Threads;
using MiroslavGPT.Domain.Models;

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

    public async Task<Guid> CreateThreadAsync(long chatId)
    {
        var thread = new CosmosThread
        {
            ChatId = chatId,
            Id = Guid.NewGuid(),
            Messages = new List<ThreadMessage>(),
        };
        await _threadsContainer.CreateItemAsync(thread, new PartitionKey(thread.Id.ToString()));
        return thread.Id;
    }

    public async Task<Guid?> GetThreadByMessageAsync(long chatId, long messageId)
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.chatId = @chatId AND ARRAY_CONTAINS(c.messages, { messageId: @messageId }, true)")
                .WithParameter("@chatId", chatId)
                .WithParameter("@messageId", messageId);
            var iterator = _threadsContainer.GetItemQueryIterator<CosmosThread>(query);
            if (!iterator.HasMoreResults)
            {
                return null;
            }

            var response = await iterator.ReadNextAsync();
            return response.FirstOrDefault()?.Id;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task AddThreadMessageAsync(Guid id, long messageId, string text, string username, bool isAssistant)
    {
        var thread = (await _threadsContainer.ReadItemAsync<CosmosThread>(id.ToString(), new PartitionKey(id.ToString()))).Resource;
        var threadMessage = new ThreadMessage
        {
            MessageId = messageId,
            Text = text,
            Username = username,
            IsAssistant = isAssistant,
        };
        thread.Messages = thread.Messages.TakeLast(_settings.ThreadLengthLimit - 1).Append(threadMessage).ToList();
        thread.Messages.Add(threadMessage);
        await _threadsContainer.ReplaceItemAsync(thread, thread.Id.ToString(), new PartitionKey(thread.Id.ToString()));
    }

    public async Task<List<ThreadMessage>> GetMessagesAsync(Guid id)
    {
        var thread = (await _threadsContainer.ReadItemAsync<CosmosThread>(id.ToString(), new PartitionKey(id.ToString()))).Resource;
        return thread.Messages;
    }

    public record CosmosThread
    {
        public long ChatId { get; set; }
        public Guid Id { get; set; }
        public List<ThreadMessage> Messages { get; set; }
    }
}