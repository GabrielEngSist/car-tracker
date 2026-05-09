namespace Car.Tracker.Api.Configuration;

internal static class CarTrackerConnectionString
{
    internal static string Resolve(IConfiguration configuration)
    {
        var dbSettings = configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>() ?? new DatabaseSettings();
        return $"Host={dbSettings.Host};Database={dbSettings.Database};Username={dbSettings.Username};Password={dbSettings.Password};Port={dbSettings.Port};SSL Mode=VerifyFull;Channel Binding=Require";
    }
}
