using System.Text.RegularExpressions;
using MiroslavGPT.Admin.Domain.Common;
using MiroslavGPT.Admin.Domain.Errors;
using MiroslavGPT.Admin.Domain.Interfaces.Personalities;
using MiroslavGPT.Model.Personalities;

namespace MiroslavGPT.Admin.Domain.Personalities;

public class PersonalityService : IPersonalityService
{
    private static readonly Regex _commandRegex = new(@"^/[a-zA-Z0-9]+$", RegexOptions.Compiled);
    
    private readonly IPersonalityRepository _personalityRepository;

    public PersonalityService(IPersonalityRepository personalityRepository)
    {
        _personalityRepository = personalityRepository;
    }

    public async Task<Result<List<Personality>>> GetPersonalitiesResultAsync()
    {
        return await Result<List<Personality>>.OkAsync(_personalityRepository.GetPersonalitiesAsync());
    }

    public async Task<Result<Personality>> GetPersonalityResultAsync(string id)
    {
        var personality = await _personalityRepository.GetPersonalityAsync(id);
        if (personality == null)
        {
            return Result<Personality>.Failure(PersonalityError.NOT_FOUND);
        }

        return Result<Personality>.Ok(personality);
    }

    public async Task<Result<Personality>> GetPersonalityByCommandResultAsync(string command)
    {
        var personality = await _personalityRepository.GetPersonalityByCommandAsync(command);
        if (personality == null)
        {
            return Result<Personality>.Failure(PersonalityError.NOT_FOUND);
        }

        return Result<Personality>.Ok(personality);
    }

    public async Task<Result<Personality>> CreatePersonalityResultAsync(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return Result<Personality>.Failure(PersonalityError.EMPTY_COMMAND);
        }
        if (!_commandRegex.IsMatch(command))
        {
            return Result<Personality>.Failure(PersonalityError.INVALID_COMMAND);
        }
        var personality = await _personalityRepository.GetPersonalityByCommandAsync(command);
        if (personality != null)
        {
            return Result<Personality>.Failure(PersonalityError.ALREADY_EXISTS);
        }
        
        return await Result<Personality>.OkAsync(_personalityRepository.CreatePersonalityAsync(command));
    }
    
    public async Task<Result> UpdatePersonalityResultAsync(string id, string command, string systemMessage)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return Result.Failure(PersonalityError.EMPTY_COMMAND);
        }
        if (!_commandRegex.IsMatch(command))
        {
            return Result.Failure(PersonalityError.INVALID_COMMAND);
        }
        var personality = await _personalityRepository.GetPersonalityByCommandAsync(command);
        if (personality != null)
        {
            return Result.Failure(PersonalityError.ALREADY_EXISTS);
        }
        await _personalityRepository.UpdatePersonalityAsync(id, command, systemMessage);
        return Result.Ok();
    }

    public async Task<Result> DeletePersonalityResultAsync(string id)
    {
        await _personalityRepository.DeletePersonalityAsync(id);
        return Result.Ok();
    }

    public async Task<Result<PersonalityMessage>> AddPersonalityMessageResultAsync(string id, string text, bool isAssistant)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result<PersonalityMessage>.Failure(PersonalityError.EMPTY_MESSAGE);
        }
        var personality = await _personalityRepository.GetPersonalityAsync(id);
        if (personality == null)
        {
            return Result<PersonalityMessage>.Failure(PersonalityError.NOT_FOUND);
        }
        var message = await _personalityRepository.AddPersonalityMessageAsync(id, text, isAssistant);
        return Result<PersonalityMessage>.Ok(message);
    }
    
    public async Task<Result> UpdatePersonalityMessageResultAsync(string id, string messageId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure(PersonalityError.EMPTY_MESSAGE);
        }
        var message = await _personalityRepository.GetPersonalityMessageAsync(id, messageId);
        if (message == null)
        {
            return Result.Failure(PersonalityError.NOT_FOUND);
        }
        await _personalityRepository.UpdatePersonalityMessageAsync(id, messageId, text);
        return Result.Ok();
    }

    public async Task<Result> DeletePersonalityMessageResultAsync(string id, string messageId)
    {
        await _personalityRepository.DeletePersonalityMessageAsync(id, messageId);
        return Result.Ok();
    }
}