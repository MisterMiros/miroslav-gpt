using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using MiroslavGPT.Azure.Settings;
using MiroslavGPT.Domain.Interfaces.Threads;
using MiroslavGPT.Domain.Models.Threads;
using Newtonsoft.Json;

namespace MiroslavGPT.Azure.Threads;

public class CosmosThreadRepository : IThreadRepository
{
    private readonly IThreadSettings _settings;
    private readonly Container _threadsContainer;

    public CosmosThreadRepository(CosmosClient client, IThreadSettings settings)
    {
        _settings = settings;
        _threadsContainer = client.GetContainer(settings.ThreadDatabaseName, settings.ThreadContainerName);
    }

    public async Task<MessageThread> CreateThreadAsync(long chatId)
    {
        var thread = new MessageThread
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            Messages = new(),
        };
        await _threadsContainer.CreateItemAsync(ToCosmos(thread), new PartitionKey(thread.Id.ToString()));
        return thread;
    }

    public async Task<MessageThread> GetThreadByMessageAsync(long chatId, int messageId)
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.chatId = @chatId AND ARRAY_CONTAINS(c.messages, { messageId: @messageId }, true)")
                .WithParameter("@chatId", chatId)
                .WithParameter("@messageId", messageId);
            var iterator = _threadsContainer.GetItemQueryIterator<CosmosMessageThread>(query);
            if (!iterator.HasMoreResults)
            {
                return null;
            }

            var response = await iterator.ReadNextAsync();
            return FromCosmos(response.FirstOrDefault());
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task UpdateThreadAsync(MessageThread messageThread)
    {
        var cosmosThread = ToCosmos(messageThread);
        cosmosThread.Messages = cosmosThread.Messages.TakeLast(_settings.ThreadLengthLimit).ToList();
        await _threadsContainer.ReplaceItemAsync(cosmosThread, cosmosThread.Id, new PartitionKey(messageThread.Id.ToString()));
    }
    
    private static MessageThread FromCosmos(CosmosMessageThread thread)
    {
        return new()
        {
            Id = Guid.Parse(thread.Id),
            ChatId = thread.ChatId,
            Messages = thread.Messages.Select(FromCosmos).ToList(),
        };
    }
    
    private static ThreadMessage FromCosmos(CosmosThreadMessage message)
    {
        return new()
        {
            MessageId = message.MessageId,
            Username = message.Username,
            Text = message.Text,
            IsAssistant = message.IsAssistant,
        };
    }
    
    private static CosmosMessageThread ToCosmos(MessageThread thread)
    {
        return new()
        {
            Id = thread.Id.ToString(),
            ChatId = thread.ChatId,
            Messages = thread.Messages.Select(ToCosmos).ToList(),
        };
    }

    private static CosmosThreadMessage ToCosmos(ThreadMessage message)
    {
        return new()
        {
            MessageId = message.MessageId,
            Username = message.Username,
            Text = message.Text,
            IsAssistant = message.IsAssistant,
        };
    }
    
    public record CosmosMessageThread
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("chatId")]
        public long ChatId { get; set; }
        [JsonProperty("messages")]
        public List<CosmosThreadMessage> Messages { get; set; } = new();
    }
    
    public record CosmosThreadMessage
    {
        [JsonProperty("messageId")]
        public int MessageId { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("isAssistant")]
        public bool IsAssistant { get; set; }
    }
}