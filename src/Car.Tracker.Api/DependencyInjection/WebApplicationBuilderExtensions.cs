using Car.Tracker.Api.Configuration;
using Car.Tracker.Application;
using Car.Tracker.Infrastructure;
using System.Text.Json.Serialization;

namespace Car.Tracker.Api.DependencyInjection;

internal static class WebApplicationBuilderExtensions
{
    internal static WebApplicationBuilder AddCarTrackerServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();

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
