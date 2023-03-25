using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using MiroslavGPT.Domain.Interfaces;

namespace MiroslavGPT.AWS
{
    public class DynamoDBUsersRepository : IUsersRepository
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly string _tableName;

        public DynamoDBUsersRepository(string region, string tableName)
        {
            _dynamoDb = new AmazonDynamoDBClient(Amazon.RegionEndpoint.GetBySystemName(region));
            _tableName = tableName;
        }

        public async Task AuthorizeUserAsync(long chatId)
        {
            var table = Table.LoadTable(_dynamoDb, _tableName);
            var item = new Document();
            item["ChatId"] = chatId;
            item["Authorized"] = true;

            await table.PutItemAsync(item);
        }

        public async Task<bool> IsAuthorizedAsync(long chatId)
        {
            var table = Table.LoadTable(_dynamoDb, _tableName);
            var document = await table.GetItemAsync(chatId);
            return document != null && document["Authorized"].AsBoolean();
        }
    }
}
