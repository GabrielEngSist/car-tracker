namespace Car.Tracker.Api.Endpoints.Cars;

internal static class CarsEndpointRegistration
{
    internal static WebApplication MapCarsEndpoints(this WebApplication app)
    {
        var cars = app.MapGroup("/api/cars");
        cars.MapCarCoreEndpoints();
        cars.MapCarFuelingEndpoints();
        cars.MapCarReportEndpoints();
        cars.MapCarExpenseEntryEndpoints();
        cars.MapCarMaintenanceEndpoints();
        return app;
    }
}
