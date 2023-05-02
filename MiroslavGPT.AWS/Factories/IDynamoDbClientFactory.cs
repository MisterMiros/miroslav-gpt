using Amazon.DynamoDBv2;

namespace MiroslavGPT.AWS.Factories
{
    public interface IDynamoDbClientFactory
    {
        public IAmazonDynamoDB CreateClient(string region);
    }
}
