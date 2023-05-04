using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.IO;
using MiroslavGPT.Domain.Interfaces;

namespace MiroslavGPT.Azure;

public class TelegramWebhookFunction
{
    private readonly ITelegramMessageHandler _telegramMessageHandler;
    private readonly ILogger<TelegramWebhookFunction> _logger;

    public TelegramWebhookFunction(ITelegramMessageHandler telegramMessageHandler, ILogger<TelegramWebhookFunction> logger)
    {
        _telegramMessageHandler = telegramMessageHandler;
        _logger = logger;
    }

    [FunctionName("TelegramWebhookFunction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "webhook")] HttpRequest req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var update = JObject.Parse(requestBody).ToObject<Update>();
            await _telegramMessageHandler.ProcessUpdateAsync(update);

            return new OkResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook request");
            return new StatusCodeResult(StatusCodes.Status200OK);
        }
    }
}