using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiroslavGPT.Domain.Interfaces;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MiroslavGPT.AWS;

public class TelegramWebhookFunction
{
    private readonly ITelegramMessageHandler _telegramMessageHandler;
    private readonly ILogger<TelegramWebhookFunction> _logger;

    public TelegramWebhookFunction()
    {
        var services = new ServiceCollection();
        Startup.ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        _telegramMessageHandler = serviceProvider.GetRequiredService<ITelegramMessageHandler>();
        _logger = serviceProvider.GetRequiredService<ILogger<TelegramWebhookFunction>>();
    }

    public TelegramWebhookFunction(ITelegramMessageHandler messageHandler, ILogger<TelegramWebhookFunction> logger)
    {
        _telegramMessageHandler = messageHandler;
        _logger = logger;
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            _logger.LogInformation("Processing webhook request");
            _logger.LogInformation(request.Body);
            var update = JObject.Parse(request.Body).ToObject<Update>();
            await _telegramMessageHandler.ProcessUpdateAsync(update);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook request");
        }

        return new APIGatewayProxyResponse { StatusCode = 200 };
    }
}