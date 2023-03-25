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
    public static class TelegramWebhookFunction
    {
        private static readonly TelegramMessageHandler _telegramMessageHandler;

        static TelegramWebhookFunction()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var secretKey = config["SECRET_KEY"];
            var openAiApiKey = config["OPENAI_API_KEY"];
            var telegramBotToken = config["TELEGRAM_BOT_TOKEN"];
            var connectionString = config["COSMOSDB_CONNECTION_STRING"];
            var databaseName = config["COSMOSDB_DATABASE_NAME"];
            var containerName = config["COSMOSDB_CONTAINER_NAME"];
            var botUsername = config["TELEGRAM_BOT_USERNAME"];
            var maxTokens = int.Parse(config["MAX_TOKENS"] ?? "100");

            var usersRepository = new CosmosDBUsersRepository(connectionString, databaseName, containerName);
            var personality = new TsunderePersonalityProvider();
            var chatGPTBot = new ChatGPTBot(secretKey, usersRepository, personality, openAiApiKey, maxTokens);

            _telegramMessageHandler = new TelegramMessageHandler(chatGPTBot, telegramBotToken, botUsername);
        }

        [FunctionName("TelegramWebhookFunction")]
        public static async Task<IActionResult> Run(
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
                logger.LogError(ex.StackTrace);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}