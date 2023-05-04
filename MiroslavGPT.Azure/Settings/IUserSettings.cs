namespace MiroslavGPT.Azure.Settings;

public interface IUserSettings
{
    public string UserDatabaseName { get; set; }
    public string UserContainerName { get; set; }
}