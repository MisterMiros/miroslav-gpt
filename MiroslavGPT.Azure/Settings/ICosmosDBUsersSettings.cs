namespace MiroslavGPT.Azure.Settings
{
    public interface ICosmosDbUsersSettings
    {
        public string UsersDatabaseName { get; set; }
        public string UsersContainerName { get; set; }
    }
}
