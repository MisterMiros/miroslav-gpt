namespace MiroslavGPT.Azure.Settings
{
    public interface ICosmosDBUsersSettings
    {
        public string UsersDatabaseName { get; set; }
        public string UsersContainerName { get; set; }
    }
}
