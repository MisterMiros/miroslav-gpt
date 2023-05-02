using MiroslavGPT.Domain.Extensions;
using MiroslavGPT.Domain.Interfaces;
using MiroslavGPT.Domain.Interfaces.Clients;
using MiroslavGPT.Domain.Interfaces.Personality;
using MiroslavGPT.Domain.Interfaces.Threads;
using MiroslavGPT.Domain.Interfaces.Users;
using MiroslavGPT.Domain.Models;
using MiroslavGPT.Domain.Models.Commands;
using MiroslavGPT.Domain.Models.Threads;
using MiroslavGPT.Domain.Settings;
using Telegram.Bot.Types;
using Thread = MiroslavGPT.Domain.Models.Threads.Thread;

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
        response = response.EscapeUsernames(usernames);

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

    private async Task<Thread> GetThreadAsync(long chatId, int? replyToId)
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