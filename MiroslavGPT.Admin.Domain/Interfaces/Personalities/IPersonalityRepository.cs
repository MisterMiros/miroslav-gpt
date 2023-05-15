using MiroslavGPT.Model.Personalities;

namespace MiroslavGPT.Admin.Domain.Interfaces.Personalities;

public interface IPersonalityRepository
{
    Task<List<Personality>> GetPersonalitiesAsync();
    Task<Personality?> GetPersonalityAsync(string id);
    Task<Personality?> GetPersonalityByCommandAsync(string command);
    Task<Personality> CreatePersonalityAsync(string command);
    Task UpdatePersonalityAsync(string id, string command);
    Task<PersonalityMessage> AddPersonalityMessageAsync(string id, string text, bool isAssistant);
    Task DeletePersonalityMessageAsync(string id, string messageId);
}