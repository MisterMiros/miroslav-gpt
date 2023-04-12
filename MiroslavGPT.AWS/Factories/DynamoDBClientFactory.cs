﻿using Amazon.DynamoDBv2;
using MiroslavGPT.AWS.Settings;

namespace MiroslavGPT.AWS.Factories
{
    public class DynamoDBClientFactory : IDynamoDBClientFactory
    {
        public IAmazonDynamoDB CreateClient(string region)
        {
            return new AmazonDynamoDBClient(Amazon.RegionEndpoint.GetBySystemName(region));
        }
    }
}
