using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using MiroslavGPT.Azure.Settings;
using MiroslavGPT.Domain.Interfaces;

namespace MiroslavGPT.Azure
{
    public class CosmosDBUsersRepository : IUsersRepository
    {
        private readonly CosmosClient _client;
        private readonly Container _container;

        public CosmosDBUsersRepository(ICosmosDBSettings cosmosDBSettings, ICosmosDBUsersSettings cosmosDBUsersSettings)
        {
            _client = new CosmosClient(cosmosDBSettings.ConnectionString);
            _container = _client.GetContainer(cosmosDBUsersSettings.UsersDatabaseName, cosmosDBUsersSettings.UsersContainerName);
        }

        private async Task<User> GetUserAsync(long userId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @userId")
                    .WithParameter("@userId", userId.ToString());

            var iterator = _container.GetItemQueryIterator<User>(query);
            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                var user = response.FirstOrDefault();
                return user;
            }
            return null;
        }

        public async Task<bool> IsAuthorizedAsync(long userId)
        {
            try
            {
                var user = await GetUserAsync(userId);
                return user?.isAuthorized ?? false;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public async Task AuthorizeUserAsync(long userId)
        {
            var user = new User(userId.ToString(), true, false);

            await _container.UpsertItemAsync(user, new PartitionKey(userId.ToString()));
        }

        public async Task<bool> IsVoiceOverEnabledAsync(long userId)
        {
            try
            {
                var user = await GetUserAsync(userId);
                return user?.voiceOver ?? false;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public async Task SetVoiceOverAsync(long userId, bool enabled)
        {
            var patch = PatchOperation.Set("voiceOver", enabled);
            await _container.PatchItemAsync<User>(userId.ToString(), new PartitionKey(userId.ToString()), new[] { patch });
        }

        private record User(string id, bool isAuthorized, bool voiceOver);
    }
}
