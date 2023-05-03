namespace MiroslavGPT.AWS.Settings;

public interface IThreadSettings
{
    public string ThreadTableName { get; set; }
    public int ThreadLengthLimit { get; set; }
}