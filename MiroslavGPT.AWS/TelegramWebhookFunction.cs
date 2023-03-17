using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MiroslavGPT.Domain;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace MiroslavGPT.AWS
{
    public class TelegramWebhookFunction
    {
        private readonly TelegramMessageHandler _telegramMessageHandler;

        public TelegramWebhookFunction()
        {
            string secretKey = Environment.GetEnvironmentVariable("SECRET_KEY");
            string openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            string telegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            string dynamoDbTableName = Environment.GetEnvironmentVariable("DYNAMODB_TABLE_NAME");
            string botUsername = Environment.GetEnvironmentVariable("TELEGRAM_BOT_USERNAME");
            int maxTokens = int.Parse(Environment.GetEnvironmentVariable("MAX_TOKENS") ?? "100");

            var dynamoDbConfig = new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION")),
            };

            var dynamoDb = new AmazonDynamoDBClient(dynamoDbConfig);
            IUsersRepository usersRepository = new DynamoDBUsersRepository(dynamoDb, dynamoDbTableName);
            ChatGPTBot chatGPTBot = new ChatGPTBot(secretKey, usersRepository, openAiApiKey, maxTokens);

            _telegramMessageHandler = new TelegramMessageHandler(chatGPTBot, telegramBotToken, botUsername);
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var update = JObject.Parse(request.Body).ToObject<Update>();
                await _telegramMessageHandler.ProcessUpdateAsync(update);

                return new APIGatewayProxyResponse { StatusCode = 200 };
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error processing webhook request: {ex.Message}");
                return new APIGatewayProxyResponse { StatusCode = 500 };
            }
        }
    }
}