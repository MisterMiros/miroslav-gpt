using Amazon.DynamoDBv2;

namespace MiroslavGPT.AWS.Factories
{
    public interface IDynamoDBClientFactory
    {
        public IAmazonDynamoDB CreateClient(string region);
    }
}
