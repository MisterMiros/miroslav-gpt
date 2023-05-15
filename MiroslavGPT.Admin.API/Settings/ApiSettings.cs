using MiroslavGPT.Admin.Domain.Interfaces.Settings;

namespace MiroslavGPT.Admin.API.Settings;

public class ApiSettings: IAuthSettings, IPersonalitySettings
{
    public string Secret { get; set; }
    public string PersonalityDatabaseName { get; set; }
    public string PersonalityContainerName { get; set; }
}