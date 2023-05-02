using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using MiroslavGPT.AWS.Factories;
using MiroslavGPT.AWS.Settings;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Interfaces.Users;

namespace MiroslavGPT.AWS;

public class DynamoDbUsersRepository : IUsersRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly IDynamoDbUsersSettings _settings;

    public DynamoDbUsersRepository(IDynamoDbClientFactory clientFactory, IRegionSettings regionSettings, IDynamoDbUsersSettings dynamoDbUsersSettings)
    {
        _dynamoDb = clientFactory.CreateClient(regionSettings.RegionName);
        _settings = dynamoDbUsersSettings;
    }

    public async Task AuthorizeUserAsync(long chatId)
    {
        var item = new Document();
        item["ChatId"] = chatId;
        item["Authorized"] = true;

        await _dynamoDb.PutItemAsync(_settings.UsersTableName, item.ToAttributeMap());
    }

    public async Task<bool> IsAuthorizedAsync(long chatId)
    {
        var item = await _dynamoDb.GetItemAsync(_settings.UsersTableName, new Dictionary<string, AttributeValue> { { "ChatId", new AttributeValue { N = chatId.ToString() }}});
        var document = Document.FromAttributeMap(item.Item);
        return document != null && document.ContainsKey("Authorized") && document["Authorized"].AsBoolean();
    }
}