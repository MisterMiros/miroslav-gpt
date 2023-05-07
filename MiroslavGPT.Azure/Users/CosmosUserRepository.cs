using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using MiroslavGPT.Azure.Settings;
using MiroslavGPT.Domain.Interfaces.Users;
using Newtonsoft.Json;

namespace MiroslavGPT.Azure.Users;

public class CosmosUserRepository : IUserRepository
{
    private readonly Container _container;

    public CosmosUserRepository(CosmosClient client, IUserSettings userSettings)
    {
        _container = client.GetContainer(userSettings.UserDatabaseName, userSettings.UserContainerName);
    }

    public async Task<bool> IsAuthorizedAsync(long userId)
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @userId")
                .WithParameter("@userId", userId.ToString());

            var iterator = _container.GetItemQueryIterator<CosmosUser>(query);

            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                var user = response.FirstOrDefault();
                return user?.IsAuthorized == true;
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
        var user = new CosmosUser
        {
            Id = userId.ToString(),
            IsAuthorized = true,
        };

        await _container.UpsertItemAsync(user, new PartitionKey(userId.ToString()));
    }

    public record CosmosUser
    {
        [JsonProperty("id")] 
        public string Id { get; set; }
        [JsonProperty("isAuthorized")] 
        public bool IsAuthorized { get; set; }
    };
}