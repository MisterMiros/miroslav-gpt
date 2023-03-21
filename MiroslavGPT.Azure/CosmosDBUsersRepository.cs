using MiroslavGPT.Domain;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace MiroslavGPT.Azure
{
    public class CosmosDBUsersRepository : IUsersRepository
    {
        private readonly CosmosClient _client;
        private readonly Container _container;

        public CosmosDBUsersRepository(string connectionString, string databaseName, string containerName)
        {
            _client = new CosmosClient(connectionString);
            _container = _client.GetContainer(databaseName, containerName);
        }

        public async Task<bool> IsAuthorizedAsync(long userId)
        {
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId")
                    .WithParameter("@userId", userId);

                var iterator = _container.GetItemQueryIterator<User>(query);

                if (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    var user = response.FirstOrDefault();

                    if (user != null)
                    {
                        return user.IsAuthorized;
                    }
                }

                return false;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public async Task AuthorizeUserAsync(long userId)
        {
            var user = new User
            {
                UserId = userId,
                IsAuthorized = true
            };

            await _container.CreateItemAsync(user, new PartitionKey(userId.ToString()));
        }

        private class User
        {
            public long UserId { get; set; }
            public bool IsAuthorized { get; set; }
        }
    }
}
