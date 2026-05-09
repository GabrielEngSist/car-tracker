using Car.Tracker.Api.Endpoints.Cars;

namespace Car.Tracker.Api.Endpoints;

internal static class CarTrackerApiExtensions
{
    internal static WebApplication MapCarTrackerApi(this WebApplication app)
    {
        app.MapHealthEndpoints();
        app.MapCarsEndpoints();
        app.MapGlobalFuelingsEndpoints();
        return app;
    }
}
