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

    public override ICommand? TryGetCommand(Update update)
    {
        _logger.LogDebug("Trying to get prompt command");
        var parts = update.Message!.Text!.Split(' ', 2);
        var command = parts[0].Replace("@" + _settings.TelegramBotUsername, string.Empty).Trim();
        var argument = parts.Length > 1 ? parts[1].Trim() : string.Empty;
        _logger.LogDebug("Prompt command is {Command} with argument {Argument}", command, argument);
        if (!_personalityProvider.HasPersonalityCommand(command))
        {
            _logger.LogDebug("Prompt command {Command} is not a valid personality command", command);
            return null;
        }
        _logger.LogDebug("Prompt command {Command} is a valid personality command", command);
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
        _logger.LogDebug("Executing prompt command {Command} for user {ChatId}", command.Personality, command.ChatId);
        if (!await _userRepository.IsAuthorizedAsync(command.ChatId))
        {
            _logger.LogDebug("User {ChatId} is not authorized", command.ChatId);
            await TelegramClient.SendTextMessageAsync(command.ChatId, "You are not authorized. Please use /init command with the correct secret key.", command.MessageId);
            return;
        }

        if (string.IsNullOrWhiteSpace(command.Prompt))
        {
            _logger.LogDebug("Prompt command {Command} does not have a prompt for user {ChatId}", command.Personality, command.ChatId);
            await TelegramClient.SendTextMessageAsync(command.ChatId, "Please provide a prompt after the personality command.", command.MessageId);
            return;
        }

        _logger.LogDebug("Getting thread for chat {ChatId} and message {ReplyToId}", command.ChatId, command.ReplyToId);
        var thread = await GetThreadAsync(command.ChatId, command.ReplyToId);
        thread.Messages.Add(new()
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

        _logger.LogDebug("Getting response from ChatGPT API for prompt {Prompt} and {Messages} messages", command.Prompt, messages.Count);
        var response = await _chatClient.GetChatGptResponseAsync(command.Prompt, messages);
        var usernames = thread.Messages.Select(m => m.Username).Distinct();
        response = response.EscapeUsernames();
        
        _logger.LogDebug("Escaped response is {Response}", response);

        _logger.LogDebug("Sending response to user {ChatId} for prompt {Prompt}", command.ChatId, command.Prompt);
        var message = await TelegramClient.SendTextMessageAsync(command.ChatId, $"*Response from ChatGPT API for prompt '{command.Prompt}':*\n\n{response}", command.MessageId);

        thread.Messages.Add(new()
        {
            MessageId = message.MessageId,
            Username = null,
            Text = response,
            IsAssistant = true,
        });

        _logger.LogDebug("Updating thread for chat {ChatId}", command.ChatId);
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