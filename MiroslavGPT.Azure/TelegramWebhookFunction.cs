using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MiroslavGPT.Domain;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using System;

namespace MiroslavGPT.Azure
{
    public static class TelegramWebhookFunction
    {
        private static readonly TelegramMessageHandler _telegramMessageHandler;

        static TelegramWebhookFunction()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            string secretKey = config["SECRET_KEY"];
            string openAiApiKey = config["OPENAI_API_KEY"];
            string telegramBotToken = config["TELEGRAM_BOT_TOKEN"];
            string connectionString = config["COSMOSDB_CONNECTION_STRING"];
            string databaseName = config["COSMOSDB_DATABASE_NAME"];
            string containerName = config["COSMOSDB_CONTAINER_NAME"];
            string botUsername = config["TELEGRAM_BOT_USERNAME"];
            int maxTokens = int.Parse(config["MAX_TOKENS"] ?? "100");

            CosmosDBUsersRepository usersRepository = new CosmosDBUsersRepository(connectionString, databaseName, containerName);
            ChatGPTBot chatGPTBot = new ChatGPTBot(secretKey, usersRepository, openAiApiKey, maxTokens);

            _telegramMessageHandler = new TelegramMessageHandler(chatGPTBot, telegramBotToken, botUsername);
        }

        [Function("TelegramWebhookFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "webhook")] HttpRequestData req,
            FunctionContext context)
        {
            var logger = context.GetLogger("TelegramWebhookFunction");

            try
            {
                var requestBody = await req.ReadAsStringAsync();
                var update = JObject.Parse(requestBody).ToObject<Update>();
                await _telegramMessageHandler.ProcessUpdateAsync(update);

                return new OkResult();
            }
            catch (Exception ex)
            {
                logger.LogError($"Error processing webhook request: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}