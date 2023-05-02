using MiroslavGPT.Domain.Extensions;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Interfaces.Clients;
using MiroslavGPT.Domain.Interfaces.Personality;
using MiroslavGPT.Domain.Interfaces.Threads;
using MiroslavGPT.Domain.Interfaces.Users;
using MiroslavGPT.Domain.Models;
using MiroslavGPT.Domain.Models.Commands;
using MiroslavGPT.Domain.Settings;
using Telegram.Bot.Types;

namespace MiroslavGPT.Domain.Actions;

public class PromptAction : BaseAction<PromptCommand>
{
    private readonly IThreadRepository _threadRepository;
    private readonly IChatClient _chatClient;
    private readonly IPersonalityProvider _personalityProvider;
    private readonly ITelegramBotSettings _settings;
    private readonly IUserRepository _userRepository;

    public PromptAction(
        IUserRepository userRepository,
        IThreadRepository threadRepository,
        IPersonalityProvider personalityProvider,
        ITelegramBotSettings settings,
        IChatClient chatClient,
        ITelegramClient telegramClient) : base(telegramClient)
    {
        _threadRepository = threadRepository;
        _chatClient = chatClient;
        _personalityProvider = personalityProvider;
        _settings = settings;
        _userRepository = userRepository;
    }

    public override PromptCommand TryGetCommand(Update update)
    {
        var parts = update!.Message!.Text!.Split(' ', 2);
        var command = parts[0].Replace("@" + _settings.TelegramBotUsername, string.Empty).Trim();
        var argument = parts.Length > 1 ? parts[1].Trim() : string.Empty;
        if (!_personalityProvider.HasPersonalityCommand(command))
        {
            return null;
        }

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

    public override async Task ExecuteAsync(PromptCommand command)
    {
        if (!await _userRepository.IsAuthorizedAsync(command.ChatId))
        {
            await TelegramClient.SendTextMessageAsync(command.ChatId, "You are not authorized. Please use /init command with the correct secret key.", command.MessageId);
        }

        if (string.IsNullOrWhiteSpace(command.Prompt))
        {
            await TelegramClient.SendTextMessageAsync(command.ChatId, "Please provide a prompt after the personality command.", command.MessageId);
        }

        var threadId = await GetThreadIdAsync(command.ChatId, command.ReplyToId);

        await _threadRepository.AddThreadMessageAsync(threadId, command.MessageId, command.Prompt, command.Username, false);

        var threadMessages = await _threadRepository.GetMessagesAsync(threadId);
        var messages = _personalityProvider
            .GetPersonalityMessages(command.Personality)
            .Concat(threadMessages.Select(m => m.ToChatMessage()))
            .ToList();

        var response = await _chatClient.GetChatGptResponseAsync(command.Prompt, messages);
        var usernames = threadMessages.Select(m => m.Username).Distinct();
        response = response.EscapeUsernames(usernames);

        var message = await TelegramClient.SendTextMessageAsync(command.ChatId, $"*Response from ChatGPT API for prompt '{command.Prompt}':*\n\n{response}", command.MessageId);

        await _threadRepository.AddThreadMessageAsync(threadId, message.MessageId, response, null, true);
    }

    private async Task<Guid> GetThreadIdAsync(long chatId, int? replyToId)
    {
        if (replyToId.HasValue)
        {
            var threadId = await _threadRepository.GetThreadByMessageAsync(chatId, replyToId.Value);
            if (threadId.HasValue)
            {
                return threadId.Value;
            }
        }

        return await _threadRepository.CreateThreadAsync(chatId);
    }
}