using Car.Tracker.Api.Configuration;
using Car.Tracker.Api.Hosting;
using Car.Tracker.Application;
using Car.Tracker.Infrastructure;
using System.Text.Json.Serialization;

namespace Car.Tracker.Api.DependencyInjection;

internal static class WebApplicationBuilderExtensions
{
    internal static WebApplicationBuilder AddCarTrackerServices(this WebApplicationBuilder builder)
    {
        // Must run before any Configure<>() so secrets pulled from Key Vault are visible during binding.
        builder.AddAzureKeyVaultIfConfigured();

        builder.Services.AddOpenApi();

        builder.Services.Configure<CanonicalHostOptions>(
            builder.Configuration.GetSection(CanonicalHostOptions.SectionName));

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.AddConsultarPlacaIntegration(builder.Configuration);
        builder.Services.AddInfrastructure(CarTrackerConnectionString.Resolve, builder.Configuration);
        builder.Services.AddApplication();

        return builder;
    }
}
