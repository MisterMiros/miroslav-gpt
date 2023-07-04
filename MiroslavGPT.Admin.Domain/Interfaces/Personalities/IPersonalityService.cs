using MiroslavGPT.Admin.Domain.Common;
using MiroslavGPT.Admin.Domain.Errors;
using MiroslavGPT.Model.Personalities;

namespace MiroslavGPT.Admin.Domain.Interfaces.Personalities;

public interface IPersonalityService
{
    Task<Result<List<Personality>, PersonalityError>> GetPersonalitiesResultAsync();
    Task<Result<Personality, PersonalityError>> GetPersonalityResultAsync(string id);
    Task<Result<Personality, PersonalityError>> GetPersonalityByCommandResultAsync(string command);
    Task<Result<Personality, PersonalityError>> CreatePersonalityResultAsync(string command);
    Task<Result<PersonalityError>> UpdatePersonalityResultAsync(string id, string command, string systemMessage);
    Task<Result<PersonalityError>> DeletePersonalityResultAsync(string id);
    Task<Result<PersonalityMessage, PersonalityError>> AddPersonalityMessageResultAsync(string id, string text, bool isAssistant);
    Task<Result<PersonalityError>> UpdatePersonalityMessageResultAsync(string id, string messageId, string text);
    Task<Result<PersonalityError>> DeletePersonalityMessageResultAsync(string id, string messageId);
}