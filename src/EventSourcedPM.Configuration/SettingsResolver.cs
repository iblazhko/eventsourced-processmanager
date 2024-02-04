namespace EventSourcedPM.Configuration;

using Microsoft.Extensions.Configuration;

public static class SettingsResolver
{
    public static ShipmentProcessSettings GetSettings()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables() // looking for env. vars with prefix "EventSourcedPM__" derived from top section name
            .Build();

        return config.GetSection(nameof(EventSourcedPM)).Get<ShipmentProcessSettings>();
    }
}
