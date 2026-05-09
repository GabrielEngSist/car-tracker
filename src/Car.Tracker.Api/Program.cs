using Car.Tracker.Api.DependencyInjection;
using Car.Tracker.Api.Endpoints;
using Car.Tracker.Api.Hosting;
using Car.Tracker.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddCarTrackerServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDeveloperExceptionPage();

await app.Services.MigrateDatabaseAsync();

app.UseCarTrackerStaticAssets();
app.MapCarTrackerApi();
app.MapCarTrackerSpaFallback();

await app.RunAsync();
