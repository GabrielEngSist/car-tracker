namespace Car.Tracker.Api.Hosting;

internal static class StaticAndSpaExtensions
{
    internal static WebApplication UseCarTrackerStaticAssets(this WebApplication app)
    {
        app.UseDefaultFiles();
        app.UseStaticFiles();
        return app;
    }

    internal static WebApplication MapCarTrackerSpaFallback(this WebApplication app)
    {
        app.MapFallbackToFile("index.html");
        return app;
    }
}
