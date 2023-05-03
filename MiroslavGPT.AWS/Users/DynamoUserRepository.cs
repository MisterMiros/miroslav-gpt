using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using MiroslavGPT.AWS.Settings;
using MiroslavGPT.Domain.Interfaces.Users;

namespace MiroslavGPT.AWS.Users;

public class DynamoUserRepository : IUserRepository
{
    private readonly IDynamoDBContext _context;
    private readonly IUserSettings _settings;

    public DynamoUserRepository(IDynamoDBContext context, IUserSettings userSettings)
    {
        _context = context;
        _settings = userSettings;
    }

    public async Task AuthorizeUserAsync(long chatId)
    {
        var item = new DynamoUser()
        {
            ChatId = chatId,
            Authorized = true,
        };

        await _context.SaveAsync(item, new DynamoDBOperationConfig
        {
            OverrideTableName = _settings.UsersTableName,
        });
    }

    public async Task<bool> IsAuthorizedAsync(long chatId)
    {
        var item = await _context.LoadAsync<DynamoUser>(chatId, new DynamoDBOperationConfig
        {
            OverrideTableName = _settings.UsersTableName,
        });
        return item is { Authorized: true };
    }

    [DynamoDBTable("user")]
    public record DynamoUser
    {
        [DynamoDBHashKey("ChatId")]
        public long ChatId { get; set; }
        
        [DynamoDBProperty("Authorized")]
        public bool Authorized { get; set; }
    }
}