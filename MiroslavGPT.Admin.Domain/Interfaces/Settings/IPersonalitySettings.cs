namespace MiroslavGPT.Admin.Domain.Interfaces.Settings;

public interface IPersonalitySettings
{
    public string PersonalityDatabaseName { get; set; }
    public string PersonalityContainerName { get; set; }
}