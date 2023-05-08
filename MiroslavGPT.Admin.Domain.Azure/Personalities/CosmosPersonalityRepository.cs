using Microsoft.Azure.Cosmos;
using MiroslavGPT.Admin.Domain.Interfaces.Personalities;
using MiroslavGPT.Admin.Domain.Interfaces.Settings;
using MiroslavGPT.Model.Personalities;
using MiroslavGPT.Model.Personality;
using Newtonsoft.Json;

namespace MiroslavGPT.Admin.Domain.Azure.Personalities;

public class CosmosPersonalityRepository: IPersonalityRepository
{
    private readonly Container _container;

    public CosmosPersonalityRepository(CosmosClient client, IPersonalitySettings settings)
    {
        _container = client.GetContainer(settings.PersonalityDatabaseName, settings.PersonalityContainerName);
    }

    public async Task<List<Personality>> GetPersonalitiesAsync()
    {
        try
        {
            return await GetPersonalitiesPrivateAsync().ToListAsync();
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new();
        }
    }

    private async IAsyncEnumerable<Personality> GetPersonalitiesPrivateAsync()
    {
        var query = new QueryDefinition("SELECT * FROM c");
        var iterator = _container.GetItemQueryIterator<CosmosPersonality>(query);
            
        while (iterator.HasMoreResults)
        {
            var personalities = await iterator.ReadNextAsync();
            foreach (var personality in personalities)
            {
                yield return FromCosmos(personality);
            }
        }
    }

    public async Task<Personality?> GetPersonalityAsync(string id)
    {
        try
        {
            var result = await _container.ReadItemAsync<CosmosPersonality>(id, new(id));
            return FromCosmos(result.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Personality?> GetPersonalityByCommandAsync(string command)
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.command = @command)")
                .WithParameter("@command", command);
            var iterator = _container.GetItemQueryIterator<CosmosPersonality>(query);
            if (!iterator.HasMoreResults)
            {
                return null;
            }

            var response = await iterator.ReadNextAsync();
            return FromCosmos(response.First());
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Personality> UpsertPersonalityAsync(Personality personality)
    {
        var cosmosPersonality = ToCosmos(personality);
        if (string.IsNullOrEmpty(personality.Id))
        {
            cosmosPersonality.Id = Guid.NewGuid().ToString();
        }
        var created = await _container.UpsertItemAsync(cosmosPersonality, new PartitionKey(cosmosPersonality.Id));
        return FromCosmos(created.Resource);
    }

    private static CosmosPersonality ToCosmos(Personality personality)
    {
        return new()
        {
            Id = personality.Id,
            Command = personality.Command,
            Messages = personality.Messages.Select(ToCosmos).ToList(),
        };
    }

    private static CosmosPersonalityMessage ToCosmos(PersonalityMessage message)
    {
        return new()
        {
            Text = message.Text,
            IsAssistant = message.IsAssistant,
        };
    }

    private static Personality FromCosmos(CosmosPersonality personality)
    {
        return new()
        {
            Id = personality.Id,
            Command = personality.Command,
            Messages = personality.Messages.Select(FromCosmos).ToList(),
        };
    }

    private static PersonalityMessage FromCosmos(CosmosPersonalityMessage message)
    {
        return new()
        {
            Text = message.Text,
            IsAssistant = message.IsAssistant,
        };
    }
    
    public record CosmosPersonality
    {
        [JsonProperty("id")] 
        public string Id { get; set; } = string.Empty;
        [JsonProperty("command")]
        public string Command { get; set; } = string.Empty;
        [JsonProperty("messages")]
        public List<CosmosPersonalityMessage> Messages { get; set; } = new();
    }
    
    public record CosmosPersonalityMessage
    {
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;
        [JsonProperty("isAssistant")]
        public bool IsAssistant { get; set; }
    }
}