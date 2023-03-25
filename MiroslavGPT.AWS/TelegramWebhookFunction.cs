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
            var secretKey = Environment.GetEnvironmentVariable("SECRET_KEY");
            var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            var telegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            var dynamoDbTableName = Environment.GetEnvironmentVariable("DYNAMODB_TABLE_NAME");
            var botUsername = Environment.GetEnvironmentVariable("TELEGRAM_BOT_USERNAME");
            var maxTokens = int.Parse(Environment.GetEnvironmentVariable("MAX_TOKENS") ?? "100");

            var region = Environment.GetEnvironmentVariable("AWS_REGION");
            var usersRepository = new DynamoDBUsersRepository(region, dynamoDbTableName);
            var personality = new TsunderePersonalityProvider();
            var chatGPTBot = new ChatGPTBot(secretKey, usersRepository, personality, openAiApiKey, maxTokens);

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
