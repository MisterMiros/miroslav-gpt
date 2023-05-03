using Microsoft.Extensions.Logging;
using MiroslavGPT.Domain.Interfaces.Clients;
using MiroslavGPT.Domain.Interfaces.Users;
using MiroslavGPT.Domain.Models.Commands;
using MiroslavGPT.Domain.Settings;
using Telegram.Bot.Types;

namespace MiroslavGPT.Domain.Actions;

public class InitAction : BaseAction
{
    private readonly IChatGptBotSettings _chatGptBotSettings;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<InitAction> _logger;

    public InitAction(
        IChatGptBotSettings chatGptBotSettings,
        IUserRepository userRepository,
        ITelegramClient telegramClient,
        ILogger<InitAction> logger) : base(telegramClient)
    {
        _chatGptBotSettings = chatGptBotSettings;
        _userRepository = userRepository;
        _logger = logger;
    }

    public override ICommand TryGetCommand(Update update)
    {
        _logger.LogDebug("Trying to get init command");
        if (update!.Message!.Text!.StartsWith("/init"))
        {
            _logger.LogDebug("Init command found");
            var parts = update!.Message!.Text!.Split(' ', 2);
            return new InitCommand
            {
                ChatId = update.Message.Chat.Id,
                MessageId = update.Message.MessageId,
                Secret = (parts[1] ?? "").Trim(),
            };
        }
        _logger.LogInformation("Init command not found");

        return null;
    }

    public override async Task ExecuteAsync(ICommand abstractCommand)
    {
        var command = (InitCommand)abstractCommand;
        if (command.Secret == _chatGptBotSettings.SecretKey)
        {
            await _userRepository.AuthorizeUserAsync(command.ChatId);
            await TelegramClient.SendTextMessageAsync(command.ChatId, "Authorization successful! You can now use prompt commands.", command.MessageId);
        }
        else
        {
            await TelegramClient.SendTextMessageAsync(command.ChatId, "Incorrect secret key. Please try again.", command.MessageId);
        }
    }
}