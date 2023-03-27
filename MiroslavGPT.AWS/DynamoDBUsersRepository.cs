using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using MiroslavGPT.AWS.Settings;
using MiroslavGPT.Domain.Interfaces;

namespace MiroslavGPT.AWS
{
    public class DynamoDBUsersRepository : IUsersRepository
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly IDynamoDBUsersSettings _settings;

        public DynamoDBUsersRepository(IRegionSettings regionSettings, IDynamoDBUsersSettings dynamoDBUsersSettings)
        {
            _dynamoDb = new AmazonDynamoDBClient(Amazon.RegionEndpoint.GetBySystemName(regionSettings.RegionName));
            _settings = dynamoDBUsersSettings;
        }

        public async Task AuthorizeUserAsync(long chatId)
        {
            var table = Table.LoadTable(_dynamoDb, _settings.UsersTableName);
            var item = new Document();
            item["ChatId"] = chatId;
            item["Authorized"] = true;

            await table.PutItemAsync(item);
        }

        public async Task<bool> IsAuthorizedAsync(long chatId)
        {
            var table = Table.LoadTable(_dynamoDb, _settings.UsersTableName);
            var document = await table.GetItemAsync(chatId);
            return document != null && document["Authorized"].AsBoolean();
        }

        public async Task<bool> IsVoiceOverEnabledAsync(long chatId)
        {
            var table = Table.LoadTable(_dynamoDb, _settings.UsersTableName);
            var document = await table.GetItemAsync(chatId);
            return document != null && document["VoiceOver"] != null && document["VoiceOver"].AsBoolean();
        }

        public async Task SetVoiceOverAsync(long chatId, bool enabled)
        {
            var table = Table.LoadTable(_dynamoDb, _settings.UsersTableName);
            var document = await table.GetItemAsync(chatId);
            if (document == null)
            {
                return;
            }

            document["VoiceOver"] = true;
            await table.UpdateItemAsync(document);
        }
    }
}
