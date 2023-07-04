using System.Text.RegularExpressions;
using MiroslavGPT.Admin.Domain.Common;
using MiroslavGPT.Admin.Domain.Errors;
using MiroslavGPT.Admin.Domain.Interfaces.Personalities;
using MiroslavGPT.Model.Personalities;
using PersonalityResult = MiroslavGPT.Admin.Domain.Common.Result<MiroslavGPT.Model.Personalities.Personality, MiroslavGPT.Admin.Domain.Errors.PersonalityError>;
using PersonalityMessageResult = MiroslavGPT.Admin.Domain.Common.Result<MiroslavGPT.Model.Personalities.PersonalityMessage, MiroslavGPT.Admin.Domain.Errors.PersonalityError>;

namespace MiroslavGPT.Admin.Domain.Personalities;

public class PersonalityService : IPersonalityService
{
    private static readonly Regex _commandRegex = new(@"^/[a-zA-Z0-9]+$", RegexOptions.Compiled);
    
    private readonly IPersonalityRepository _personalityRepository;

    public PersonalityService(IPersonalityRepository personalityRepository)
    {
        _personalityRepository = personalityRepository;
    }

    public async Task<Result<List<Personality>, PersonalityError>> GetPersonalitiesResultAsync()
    {
        return await Result.OkAsync<List<Personality>, PersonalityError>(_personalityRepository.GetPersonalitiesAsync());
    }

    public async Task<Result<Personality, PersonalityError>> GetPersonalityResultAsync(string id)
    {
        var personality = await _personalityRepository.GetPersonalityAsync(id);
        if (personality == null)
        {
            return PersonalityResult.Failure(PersonalityError.NotFound);
        }

        return PersonalityResult.Ok(personality);
    }

    public async Task<Result<Personality, PersonalityError>> GetPersonalityByCommandResultAsync(string command)
    {
        var personality = await _personalityRepository.GetPersonalityByCommandAsync(command);
        if (personality == null)
        {
            return PersonalityResult.Failure(PersonalityError.NotFound);
        }

        return PersonalityResult.Ok(personality);
    }

    public async Task<Result<Personality, PersonalityError>> CreatePersonalityResultAsync(string command)
    {
        var personality = await _personalityRepository.GetPersonalityByCommandAsync(command);
        if (personality != null)
        {
            return PersonalityResult.Failure(PersonalityError.AlreadyExists);
        }
        
        return await PersonalityResult.OkAsync(_personalityRepository.CreatePersonalityAsync(command));
    }
    
    public async Task<Result<PersonalityError>> UpdatePersonalityResultAsync(string id, string command, string systemMessage)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return Result.Failure(PersonalityError.EmptyCommand);
        }
        if (!_commandRegex.IsMatch(command))
        {
            return Result.Failure(PersonalityError.InvalidCommand);
        }
        var personality = await _personalityRepository.GetPersonalityByCommandAsync(command);
        if (personality != null)
        {
            return Result.Failure(PersonalityError.AlreadyExists);
        }
        await _personalityRepository.UpdatePersonalityAsync(id, command, systemMessage);
        return Result.Ok<PersonalityError>();
    }

    public async Task<Result<PersonalityError>> DeletePersonalityResultAsync(string id)
    {
        await _personalityRepository.DeletePersonalityAsync(id);
        return Result.Ok<PersonalityError>();
    }

    public async Task<Result<PersonalityMessage, PersonalityError>> AddPersonalityMessageResultAsync(string id, string text, bool isAssistant)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return PersonalityMessageResult.Failure(PersonalityError.EmptyMessage);
        }
        var personality = await _personalityRepository.GetPersonalityAsync(id);
        if (personality == null)
        {
            return PersonalityMessageResult.Failure(PersonalityError.NotFound);
        }
        var message = await _personalityRepository.AddPersonalityMessageAsync(id, text, isAssistant);
        return PersonalityMessageResult.Ok(message);
    }
    
    public async Task<Result<PersonalityError>> UpdatePersonalityMessageResultAsync(string id, string messageId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure(PersonalityError.EmptyMessage);
        }
        var message = await _personalityRepository.GetPersonalityMessage(id, messageId);
        if (message == null)
        {
            return Result.Failure(PersonalityError.NotFound);
        }
        await _personalityRepository.UpdatePersonalityMessageAsync(id, messageId, text);
        return Result.Ok<PersonalityError>();
    }

    public async Task<Result<PersonalityError>> DeletePersonalityMessageResultAsync(string id, string messageId)
    {
        await _personalityRepository.DeletePersonalityMessageAsync(id, messageId);
        return Result.Ok<PersonalityError>();
    }
}