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

            string region = Environment.GetEnvironmentVariable("AWS_REGION");
            IUsersRepository usersRepository = new DynamoDBUsersRepository(region, dynamoDbTableName);
            ITranslator translator = new AmazonTranslator(region);

            // Pass the translator to the ChatGPTBot constructor
            ChatGPTBot chatGPTBot = new ChatGPTBot(secretKey, usersRepository, openAiApiKey, maxTokens, translator);

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
