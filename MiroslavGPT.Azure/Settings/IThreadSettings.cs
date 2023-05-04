namespace MiroslavGPT.Azure.Settings;

public interface IThreadSettings
{
    public string ThreadDatabaseName { get; set; }
    public string ThreadContainerName { get; set; }
    public int ThreadLengthLimit { get; set; }
}