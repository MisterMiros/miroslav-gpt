using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using MiroslavGPT.AWS.Settings;
using MiroslavGPT.Domain.Interfaces.Threads;
using MiroslavGPT.Domain.Models.Threads;

namespace MiroslavGPT.AWS.Threads;

public class DynamoThreadRepository: IThreadRepository
{
    private readonly IDynamoDBContext _context;
    private readonly IThreadSettings _settings;

    public DynamoThreadRepository(IDynamoDBContext context, IThreadSettings settings)
    {
        _context = context;
        _settings = settings;
    }
    public async Task<MessageThread> CreateThreadAsync(long chatId)
    {
        var thread = new MessageThread
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
        };
        await _context.SaveAsync(ToDynamo(thread), new DynamoDBOperationConfig
        {
            OverrideTableName = _settings.ThreadTableName,
        });
        return thread;
    }

    public async Task<MessageThread> GetThreadByMessageAsync(long chatId, int messageId)
    {
        var conditions = new[]
        {
            new ScanCondition("ChatId", ScanOperator.Equal, chatId),
            new ScanCondition("MessageIds", ScanOperator.Contains, messageId),
        };
            
        var scan = _context.ScanAsync<DynamoThread>(conditions, new DynamoDBOperationConfig
        {
            OverrideTableName = _settings.ThreadTableName,
        });

        var result = await scan.GetRemainingAsync();
        return FromDynamo(result.FirstOrDefault());
    }

    public Task UpdateThreadAsync(MessageThread messageThread)
    {
        return _context.SaveAsync(ToDynamo(messageThread), new DynamoDBOperationConfig
        {
            OverrideTableName = _settings.ThreadTableName,
        });
    }

    [DynamoDBTable("thread")]
    public record DynamoThread
    {
        [DynamoDBHashKey("Id")]
        public Guid Id { get; set; }
        
        [DynamoDBProperty("ChatId")]
        public long ChatId { get; set; }
        
        [DynamoDBProperty("MessageIds")]
        public List<int> MessageIds { get; set; }
        
        [DynamoDBProperty("Messages")]
        public List<ThreadMessage> Messages { get; set; } = new();
    }

    private static MessageThread FromDynamo(DynamoThread thread)
    {
        if (thread == null)
        {
            return null;
        }
        return new MessageThread
        {
            Id = thread.Id,
            ChatId = thread.ChatId,
            Messages = thread.Messages,
        };
    }
    
    private static DynamoThread ToDynamo(MessageThread messageThread)
    {
        return new DynamoThread
        {
            Id = messageThread.Id,
            ChatId = messageThread.ChatId,
            MessageIds = messageThread.Messages.Select(m => m.MessageId).ToList(),
            Messages = messageThread.Messages,
        };
    } 
}