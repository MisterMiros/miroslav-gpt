using Microsoft.Extensions.Logging;
using MiroslavGPT.Domain.Extensions;
using MiroslavGPT.Domain.Interfaces.Clients;
using MiroslavGPT.Domain.Interfaces.Personality;
using MiroslavGPT.Domain.Interfaces.Threads;
using MiroslavGPT.Domain.Interfaces.Users;
using MiroslavGPT.Domain.Models.Commands;
using MiroslavGPT.Domain.Models.Threads;
using MiroslavGPT.Domain.Settings;
using Telegram.Bot.Types;

namespace MiroslavGPT.Domain.Actions;

public class PromptAction : BaseAction
{
    private readonly IThreadRepository _threadRepository;
    private readonly IChatClient _chatClient;
    private readonly ILogger<PromptAction> _logger;
    private readonly IPersonalityProvider _personalityProvider;
    private readonly ITelegramBotSettings _settings;
    private readonly IUserRepository _userRepository;

    public PromptAction(
        IUserRepository userRepository,
        IThreadRepository threadRepository,
        IPersonalityProvider personalityProvider,
        ITelegramBotSettings settings,
        IChatClient chatClient,
        ITelegramClient telegramClient,
        ILogger<PromptAction> logger) : base(telegramClient)
    {
        _threadRepository = threadRepository;
        _chatClient = chatClient;
        _logger = logger;
        _personalityProvider = personalityProvider;
        _settings = settings;
        _userRepository = userRepository;
    }

    public override ICommand TryGetCommand(Update update)
    {
        _logger.LogInformation("Trying to get prompt command");
        var parts = update!.Message!.Text!.Split(' ', 2);
        var command = parts[0].Replace("@" + _settings.TelegramBotUsername, string.Empty).Trim();
        var argument = parts.Length > 1 ? parts[1].Trim() : string.Empty;
        _logger.LogInformation("Prompt command is {command} with argument {argument}", command, argument);
        if (!_personalityProvider.HasPersonalityCommand(command))
        {
            _logger.LogInformation("Prompt command {command} is not a valid personality command", command);
            return null;
        }
        _logger.LogInformation("Prompt command {command} is a valid personality command", command);
        return new PromptCommand
        {
            ChatId = update.Message.Chat.Id,
            MessageId = update.Message.MessageId,
            Username = update.Message.From!.Username,
            Personality = command,
            Prompt = argument,
            ReplyToId = update.Message.ReplyToMessage?.MessageId,
        };
    }

    public override async Task ExecuteAsync(ICommand abstractCommand)
    {
        var command = (PromptCommand)abstractCommand;
        if (!await _userRepository.IsAuthorizedAsync(command.ChatId))
        {
            await TelegramClient.SendTextMessageAsync(command.ChatId, "You are not authorized. Please use /init command with the correct secret key.", command.MessageId);
        }

        if (string.IsNullOrWhiteSpace(command.Prompt))
        {
            await TelegramClient.SendTextMessageAsync(command.ChatId, "Please provide a prompt after the personality command.", command.MessageId);
        }

        var thread = await GetThreadAsync(command.ChatId, command.ReplyToId);
        thread.Messages.Add(new ThreadMessage
        {
            MessageId = command.MessageId,
            Username = command.Username,
            Text = command.Prompt,
            IsAssistant = false,
        });
        
        var messages = _personalityProvider
            .GetPersonalityMessages(command.Personality)
            .Concat(thread.Messages.Select(m => m.ToChatMessage()))
            .ToList();

        var response = await _chatClient.GetChatGptResponseAsync(command.Prompt, messages);
        var usernames = thread.Messages.Select(m => m.Username).Distinct();
        response = response.EscapeUsernames(usernames.Where(u => u != null));

        var message = await TelegramClient.SendTextMessageAsync(command.ChatId, $"*Response from ChatGPT API for prompt '{command.Prompt}':*\n\n{response}", command.MessageId);

        thread.Messages.Add(new ThreadMessage
        {
            MessageId = message.MessageId,
            Username = null,
            Text = response,
            IsAssistant = true,
        });

        await _threadRepository.UpdateThreadAsync(thread);
    }

    private async Task<MessageThread> GetThreadAsync(long chatId, int? replyToId)
    {
        if (replyToId.HasValue)
        {
            var thread = await _threadRepository.GetThreadByMessageAsync(chatId, replyToId.Value);
            if (thread != null)
            {
                return thread;
            }
        }

        return await _threadRepository.CreateThreadAsync(chatId);
    }
}