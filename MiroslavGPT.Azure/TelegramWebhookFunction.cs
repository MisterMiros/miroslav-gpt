using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MiroslavGPT.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace MiroslavGPT.Azure
{
    public class TelegramWebhookFunction
    {
        private readonly TelegramMessageHandler _telegramMessageHandler;

        public TelegramWebhookFunction()
        {
            string endpointUri = Environment.GetEnvironmentVariable("COSMOS_ENDPOINT_URI");
            string primaryKey = Environment.GetEnvironmentVariable("COSMOS_PRIMARY_KEY");
            string databaseId = Environment.GetEnvironmentVariable("COSMOS_DATABASE_ID");
            string containerId = Environment.GetEnvironmentVariable("COSMOS_CONTAINER_ID");
            string secretKey = Environment.GetEnvironmentVariable("SECRET_KEY");
            string openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            string telegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            string botUsername = Environment.GetEnvironmentVariable("TELEGRAM_BOT_USERNAME");
            int maxTokens = int.Parse(Environment.GetEnvironmentVariable("MAX_TOKENS") ?? "100");

            IUsersRepository usersRepository = new CosmosDBUsersRepository(endpointUri, primaryKey, databaseId, containerId);
            ChatGPTBot chatGPTBot = new ChatGPTBot(secretKey, usersRepository, openAiApiKey, maxTokens);

            _telegramMessageHandler = new TelegramMessageHandler(chatGPTBot, telegramBotToken, botUsername);
        }

        public async Task<IActionResult> Run(HttpRequest req, ILogger log)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var update = JsonConvert.DeserializeObject<Update>(requestBody);
                await _telegramMessageHandler.ProcessUpdateAsync(update);

                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError($"Error processing webhook request: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
