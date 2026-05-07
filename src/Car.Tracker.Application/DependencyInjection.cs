using Car.Tracker.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Car.Tracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICarTrackerService, CarTrackerService>();
        return services;
    }
}
