using MiroslavGPT.Model.Personalities;

namespace MiroslavGPT.Admin.Domain.Interfaces.Personalities;

public interface IPersonalityRepository
{
    Task<List<Personality>> GetPersonalitiesAsync();
    Task<Personality?> GetPersonalityAsync(string id);
    Task<Personality?> GetPersonalityByCommandAsync(string command);
    Task<Personality> UpsertPersonalityAsync(Personality personality);
}