namespace MiroslavGPT.Azure.Settings
{
    public interface ICosmosUsersSettings
    {
        public string UsersDatabaseName { get; set; }
        public string UsersContainerName { get; set; }
    }
}
