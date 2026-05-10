using Azure.Identity;

namespace Car.Tracker.Api.Configuration;

internal static class KeyVaultConfigurationExtensions
{
    // Reads KeyVault:Url from the existing IConfiguration. When set, registers Azure Key Vault
    // as an additional configuration source backed by DefaultAzureCredential:
    //   - Azure-hosted: the attached User-Assigned Managed Identity is used.
    //   - Local dev: falls back to AzureCliCredential after `az login`.
    // Secret names like "CarTracker--Password" are auto-mapped to the config key "CarTracker:Password",
    // so no consumer code (DatabaseSettings / ConsultarPlacaOptions) needs to change.
    internal static WebApplicationBuilder AddAzureKeyVaultIfConfigured(this WebApplicationBuilder builder)
    {
        var url = builder.Configuration["KeyVault:Url"];
        if (string.IsNullOrWhiteSpace(url))
        {
            return builder;
        }

        builder.Configuration.AddAzureKeyVault(new Uri(url), new DefaultAzureCredential());
        return builder;
    }
}
