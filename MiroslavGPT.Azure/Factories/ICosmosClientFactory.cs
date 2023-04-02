using Microsoft.Azure.Cosmos;

namespace MiroslavGPT.Azure.Factories
{
    public interface ICosmosClientFactory
    {
        CosmosClient CreateCosmosClient(string connectionString);
    }
}
