using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Interfaces.Clients;
using MiroslavGPT.Domain.Interfaces.Users;
using MiroslavGPT.Domain.Models;
using MiroslavGPT.Domain.Models.Commands;
using MiroslavGPT.Domain.Settings;
using Telegram.Bot.Types;

namespace MiroslavGPT.Domain.Actions;

public class InitAction : BaseAction<InitCommand>
{
    private readonly IChatGptBotSettings _chatGptBotSettings;
    private readonly IUserRepository _userRepository;

    public InitAction(
        IChatGptBotSettings chatGptBotSettings,
        IUserRepository userRepository,
        ITelegramClient telegramClient) : base(telegramClient)
    {
        _chatGptBotSettings = chatGptBotSettings;
        _userRepository = userRepository;
    }

    public override InitCommand TryGetCommand(Update update)
    {
        if (update!.Message!.Text!.StartsWith("/init"))
        {
            var parts = update!.Message!.Text!.Split(' ', 2);
            return new InitCommand
            {
                ChatId = update.Message.Chat.Id,
                MessageId = update.Message.MessageId,
                Secret = (parts[1] ?? "").Trim(),
            };
        }

        return null;
    }

    public override async Task ExecuteAsync(InitCommand command)
    {
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