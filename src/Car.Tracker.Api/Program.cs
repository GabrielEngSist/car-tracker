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

// Runs BEFORE static files / endpoints so apex requests are 301'd to the canonical subdomain
// without ever serving wwwroot under the wrong hostname.
app.UseCanonicalHost();

await app.Services.MigrateDatabaseAsync();

app.UseCarTrackerStaticAssets();
app.MapCarTrackerApi();
app.MapCarTrackerSpaFallback();

await app.RunAsync();
