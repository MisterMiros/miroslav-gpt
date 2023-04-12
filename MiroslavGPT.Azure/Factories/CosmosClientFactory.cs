using Microsoft.Azure.Cosmos;

namespace MiroslavGPT.Azure.Factories
{
    public class CosmosClientFactory : ICosmosClientFactory
    {
        public CosmosClient CreateCosmosClient(string connectionString)
        {
            return new CosmosClient(connectionString);
        }
    }
}
