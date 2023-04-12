using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using MiroslavGPT.Domain.Interfaces;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace MiroslavGPT.AWS
{
    public class TelegramWebhookFunction
    {
        private readonly ITelegramMessageHandler _telegramMessageHandler;

        public TelegramWebhookFunction()
        {
            var services = new ServiceCollection();
            Startup.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            _telegramMessageHandler = serviceProvider.GetRequiredService<ITelegramMessageHandler>();
        }

        public TelegramWebhookFunction(ITelegramMessageHandler messageHandler)
        {
            _telegramMessageHandler = messageHandler;
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
                context.Logger.LogError($"Error processing webhook request: {ex}");
                return new APIGatewayProxyResponse { StatusCode = 500 };
            }
        }
    }
}
