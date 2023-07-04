using MiroslavGPT.Admin.Domain.Common;
using MiroslavGPT.Admin.Domain.Errors;
using MiroslavGPT.Model.Personalities;

namespace MiroslavGPT.Admin.Domain.Interfaces.Personalities;

public interface IPersonalityService
{
    Task<Result<List<Personality>>> GetPersonalitiesResultAsync();
    Task<Result<Personality>> GetPersonalityResultAsync(string id);
    Task<Result<Personality>> GetPersonalityByCommandResultAsync(string command);
    Task<Result<Personality>> CreatePersonalityResultAsync(string command);
    Task<Result> UpdatePersonalityResultAsync(string id, string command, string systemMessage);
    Task<Result> DeletePersonalityResultAsync(string id);
    Task<Result<PersonalityMessage>> AddPersonalityMessageResultAsync(string id, string text, bool isAssistant);
    Task<Result> UpdatePersonalityMessageResultAsync(string id, string messageId, string text);
    Task<Result> DeletePersonalityMessageResultAsync(string id, string messageId);
}