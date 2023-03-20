using MiroslavGPT.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace MiroslavGPT.Azure
{
    public class CosmosDBUsersRepository : IUsersRepository
    {
        private readonly Container _container;

        public CosmosDBUsersRepository(string endpointUri, string primaryKey, string databaseId, string containerId)
        {
            var client = new CosmosClient(endpointUri, primaryKey);
            var database = client.GetDatabase(databaseId);
            _container = database.GetContainer(containerId);
        }

        public async Task AuthorizeUserAsync(long chatId)
        {
            var item = new UserDocument
            {
                ChatId = chatId,
                Authorized = true
            };

            await _container.CreateItemAsync(item);
        }

        public async Task<bool> IsAuthorizedAsync(long chatId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.ChatId = @chatId AND c.Authorized = true")
                .WithParameter("@chatId", chatId);

            var iterator = _container.GetItemQueryIterator<UserDocument>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                foreach (var item in response)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
