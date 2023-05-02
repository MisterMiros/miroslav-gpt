namespace MiroslavGPT.Azure.Settings
{
    public interface ICosmosUserSettings
    {
        public string UserDatabaseName { get; set; }
        public string UserContainerName { get; set; }
    }
}
