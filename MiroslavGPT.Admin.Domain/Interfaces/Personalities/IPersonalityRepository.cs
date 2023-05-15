using MiroslavGPT.Model.Personalities;

namespace MiroslavGPT.Admin.Domain.Interfaces.Personalities;

public interface IPersonalityRepository
{
    Task<List<Personality>> GetPersonalitiesAsync();
    Task<Personality?> GetPersonalityAsync(string id);
    Task<Personality?> GetPersonalityByCommandAsync(string command);
    Task<Personality> InsertPersonalityAsync(Personality personality);
    Task UpdatePersonalityAsync(string id, string command);
    Task AddPersonalityMessageAsync(string id, PersonalityMessage message);
    Task DeletePersonalityMessageAsync(string id, string messageId);
}