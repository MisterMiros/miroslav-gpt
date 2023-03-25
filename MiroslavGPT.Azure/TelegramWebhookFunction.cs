using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MiroslavGPT.Domain;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.IO;

namespace MiroslavGPT.Azure
{
    public class TelegramWebhookFunction
    {
        private readonly TelegramMessageHandler _telegramMessageHandler;

        public TelegramWebhookFunction(TelegramMessageHandler telegramMessageHandler)
        {
            _telegramMessageHandler = telegramMessageHandler;
        }

        [FunctionName("TelegramWebhookFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "webhook")] HttpRequest req,
            ILogger logger)
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
                logger.LogError($"Error processing webhook request: {ex}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}