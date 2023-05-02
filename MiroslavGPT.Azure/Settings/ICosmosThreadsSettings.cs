namespace MiroslavGPT.Azure.Settings;

public interface ICosmosThreadsSettings
{
    public string ThreadsDatabaseName { get; set; }
    public string ThreadsContainerName { get; set; }
    public int ThreadLengthLimit { get; set; }
}