using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using MiroslavGPT.Azure.Factories;
using MiroslavGPT.Azure.Settings;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Interfaces.Users;

namespace MiroslavGPT.Azure;

public class CosmosDBUsersRepository : IUsersRepository
{
    private readonly CosmosClient _client;
    private readonly Container _container;

    public CosmosDBUsersRepository(ICosmosDBSettings cosmosDBSettings, ICosmosDBUsersSettings cosmosDBUsersSettings, ICosmosClientFactory cosmosClientFactory)
    {
        _client = cosmosClientFactory.CreateCosmosClient(cosmosDBSettings.ConnectionString);
        _container = _client.GetContainer(cosmosDBUsersSettings.UsersDatabaseName, cosmosDBUsersSettings.UsersContainerName);
    }

    public async Task<bool> IsAuthorizedAsync(long userId)
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @userId")
                .WithParameter("@userId", userId.ToString());

            var iterator = _container.GetItemQueryIterator<User>(query);

            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                var user = response.FirstOrDefault();
                return user.isAuthorized;
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
        var user = new User(userId.ToString(), true);

        await _container.UpsertItemAsync(user, new PartitionKey(userId.ToString()));
    }

    public record User(string id, bool isAuthorized);
}