using MiroslavGPT.Azure.Factories;

namespace MiroslavGPT.Azure.Tests
{
    public class CosmosClientFactoryTests
    {
        private CosmosClientFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _factory = new CosmosClientFactory();
        }

        [Test]
        public void CreateCosmosClient_ShouldWork() {
            // Arrange
            var endpoint = "AccountEndpoint=https://fake-cosmosdb.documents.azure.com:443/;AccountKey=Tm93QTEyMzRmYWtla29zdW1pZGJzZWNyZXQyMzR1Njg5MHg2N2Rmczg5MjBsdHJ4ZnR5MnF3MTIzNHB3OTg3YmxnOw==;";
            
            // Act
            var client = _factory.CreateCosmosClient(endpoint);

            // Assert
            client.Should().NotBeNull();
        }
    }
}
