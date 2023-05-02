namespace MiroslavGPT.Azure.Settings;

public interface ICosmosThreadSettings
{
    public string ThreadDatabaseName { get; set; }
    public string ThreadContainerName { get; set; }
    public int ThreadLengthLimit { get; set; }
}