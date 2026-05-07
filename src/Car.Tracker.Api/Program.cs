using Car.Tracker.Api.Api;
using Car.Tracker.Api.Configuration;
using Car.Tracker.Api.ConsultarPlacaModels;
using Car.Tracker.Api.Domain;
using Car.Tracker.Application.Services;
using Car.Tracker.Contracts;
using Car.Tracker.Infrastructure;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<ConsultarPlacaOptions>(
    builder.Configuration.GetSection(ConsultarPlacaOptions.SectionName));
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection(DatabaseSettings.SectionName));

builder.Services.AddTransient<ConsultarPlacaAuthHandler>();
builder.Services.AddHttpClient<IConsultarPlacaClient, ConsultarPlacaHttpClient>((sp, client) =>
{
    var o = sp.GetRequiredService<IOptions<ConsultarPlacaOptions>>().Value;
    var url = o.Url?.Trim();
    if (string.IsNullOrWhiteSpace(url))
        return;
    client.BaseAddress = new Uri(url.EndsWith('/') ? url : url + "/", UriKind.Absolute);
})
.AddHttpMessageHandler<ConsultarPlacaAuthHandler>();

builder.Services.AddHttpClient<IConsultarPrecoFipeClient, ConsultarPrecoFipeHttpClient>((sp, client) =>
{
    var o = sp.GetRequiredService<IOptions<ConsultarPlacaOptions>>().Value;
    var url = o.Url?.Trim();
    if (string.IsNullOrWhiteSpace(url))
        return;
    client.BaseAddress = new Uri(url.EndsWith('/') ? url : url + "/", UriKind.Absolute);
})
.AddHttpMessageHandler<ConsultarPlacaAuthHandler>();

var dbSettings = builder.Configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>() ?? new DatabaseSettings();
var connectionString = $"Host={dbSettings.Host};Database={dbSettings.Database};Username={dbSettings.Username};Password={dbSettings.Password};Port={dbSettings.Port};SSL Mode=VerifyFull;Channel Binding=Require";
builder.Services.AddInfrastructure(connectionString);

builder.Services.AddScoped<ICarTrackerService, CarTrackerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDeveloperExceptionPage();

await app.Services.MigrateDatabaseAsync();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }))
    .WithName("Health");

var cars = app.MapGroup("/api/cars");

cars.MapGet("/", async (ICarTrackerService svc, CancellationToken ct) =>
    Results.Ok(await svc.GetCarsAsync(ct)));

cars.MapPost("/", async (
    CreateCarRequest request,
    ICarTrackerService svc,
    IConsultarPlacaClient consultarPlaca,
    IConsultarPrecoFipeClient consultarPrecoFipe,
    CancellationToken cancellationToken) =>
{
    if (request.CurrentKm < 0) return Results.BadRequest("CurrentKm is invalid.");

    if (request.AutoBuscarDados)
    {
        if (string.IsNullOrWhiteSpace(request.Placa))
            return Results.BadRequest("Placa is required for automatic registration.");

        var placaNorm = PlacaBrasil.Normalizar(request.Placa);
        if (!PlacaBrasil.EhValida(placaNorm))
            return Results.BadRequest("Invalid plate format (use Mercosul or old Brazilian format).");

        ConsultarPlacaResponse? rPlaca;
        ConsultarPrecoFipeResponse? rFipe;
        try
        {
            rPlaca = await consultarPlaca.ConsultarPorPlacaAsync(placaNorm, cancellationToken);
            rFipe = await consultarPrecoFipe.ConsultarPorPlacaAsync(placaNorm, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }

        if (rPlaca is null || !string.Equals(rPlaca.Status, "ok", StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest($"consultarPlaca failed: {rPlaca?.Mensagem ?? "empty response"}");

        if (rFipe is null || !string.Equals(rFipe.Status, "ok", StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest($"consultarPrecoFipe failed: {rFipe?.Mensagem ?? "empty response"}");

        var car = new CarEntity { Model = "temp", Year = 2000, CurrentKm = request.CurrentKm };
        ConsultarPlacaMapper.PreencherCarro(car, rPlaca, request.CurrentKm, request.Name, placaNorm);

        car.ConsultaPlaca = ConsultarPlacaMapper.ToConsultaPlaca(car.Id, rPlaca);
        car.ConsultaPrecoFipe = ConsultarPrecoFipeMapper.ToConsultaPrecoFipe(car.Id, rFipe);

        var created = await svc.CreateCarAsync(car, cancellationToken);
        return Results.Created($"/api/cars/{created.Id}", created);
    }

    if (string.IsNullOrWhiteSpace(request.Model)) return Results.BadRequest("Model is required.");
    if (request.Year is null or < 1900 or > 3000) return Results.BadRequest("Year is invalid.");

    string? placaManual = null;
    if (!string.IsNullOrWhiteSpace(request.Placa))
    {
        placaManual = PlacaBrasil.Normalizar(request.Placa);
        if (!PlacaBrasil.EhValida(placaManual))
            return Results.BadRequest("Invalid plate format.");
    }

    var carManual = new CarEntity
    {
        Model = request.Model.Trim(),
        Year = request.Year.Value,
        CurrentKm = request.CurrentKm,
        Name = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name.Trim(),
        Placa = placaManual,
    };

    var createdManual = await svc.CreateCarAsync(carManual, cancellationToken);
    return Results.Created($"/api/cars/{createdManual.Id}", createdManual);
});

cars.MapGet("/{carId:guid}", async (Guid carId, ICarTrackerService svc, CancellationToken ct) =>
{
    var car = await svc.GetCarAsync(carId, ct);
    return car is null ? Results.NotFound() : Results.Ok(car);
});

cars.MapGet("/{carId:guid}/registry", async (Guid carId, ICarTrackerService svc, CancellationToken ct) =>
{
    var registry = await svc.GetCarRegistryAsync(carId, ct);
    return registry is null ? Results.NotFound() : Results.Ok(registry);
});

cars.MapPatch("/{carId:guid}", async (Guid carId, UpdateCarRequest request, ICarTrackerService svc, CancellationToken ct) =>
{
    try
    {
        var updated = await svc.UpdateCarAsync(carId, request, ct);
        return updated is null ? Results.NotFound() : Results.Ok(updated);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

cars.MapDelete("/{carId:guid}", async (Guid carId, ICarTrackerService svc, CancellationToken ct) =>
    await svc.DeleteCarAsync(carId, ct) ? Results.NoContent() : Results.NotFound());

cars.MapGet("/{carId:guid}/fuelings", async (Guid carId, ICarTrackerService svc, CancellationToken ct) =>
{
    var items = await svc.GetCarFuelingsAsync(carId, ct);
    return items is null ? Results.NotFound() : Results.Ok(items);
});

cars.MapGet("/{carId:guid}/reports/fuel-full-tank", async (
    Guid carId,
    string? basis,
    string? period,
    ICarTrackerService svc,
    CancellationToken ct) =>
{
    if (!CostPerKmReportQuery.ParseBasis(basis, out var lifetimeMode))
        return Results.BadRequest("basis must be 'period' or 'lifetime'.");

    PeriodAggregator periodAgg = PeriodAggregator.Total;
    if (!lifetimeMode)
    {
        if (!CostPerKmReportQuery.TryParsePeriod(period, out periodAgg))
            return Results.BadRequest("period invalid. Use total, 1d, 1m, 6m, 1y.");
    }

    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var report = await svc.GetFuelFullTankReportAsync(carId, lifetimeMode, periodAgg, today, ct);
    return report is null ? Results.NotFound() : Results.Ok(report);
});

cars.MapGet("/{carId:guid}/reports/cost-per-km", async (
    Guid carId,
    string? basis,
    string? period,
    string? distanceRef,
    ICarTrackerService svc,
    CancellationToken ct) =>
{
    if (!CostPerKmReportQuery.ParseBasis(basis, out var lifetimeMode))
        return Results.BadRequest("basis must be 'period' or 'lifetime'.");

    if (!CostPerKmReportQuery.TryParseDistanceRef(distanceRef, out var dMult))
        return Results.BadRequest("distanceRef invalid. Use total, km1, km10, km100, km1000 (or 1,10,100,1000).");

    PeriodAggregator periodAgg = PeriodAggregator.Total;
    if (!lifetimeMode)
    {
        if (!CostPerKmReportQuery.TryParsePeriod(period, out periodAgg))
            return Results.BadRequest("period invalid. Use total, 1d, 1m, 6m, 1y.");
    }

    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var report = await svc.GetCostPerKmReportAsync(carId, lifetimeMode, periodAgg, dMult, today, ct);
    return report is null ? Results.NotFound() : Results.Ok(report);
});

cars.MapPost("/{carId:guid}/fuelings", async (Guid carId, CreateFuelingEntryRequest request, ICarTrackerService svc, CancellationToken ct) =>
{
    try
    {
        var created = await svc.CreateFuelingAsync(carId, request, ct);
        return created is null ? Results.NotFound() : Results.Created($"/api/cars/{carId}/fuelings/{created.Id}", created);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

cars.MapPatch("/{carId:guid}/fuelings/{fuelingId:guid}", async (Guid carId, Guid fuelingId, UpdateFuelingEntryRequest request, ICarTrackerService svc, CancellationToken ct) =>
{
    try
    {
        var updated = await svc.UpdateFuelingAsync(carId, fuelingId, request, ct);
        return updated is null ? Results.NotFound() : Results.Ok(updated);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

cars.MapDelete("/{carId:guid}/fuelings/{fuelingId:guid}", async (Guid carId, Guid fuelingId, ICarTrackerService svc, CancellationToken ct) =>
    await svc.DeleteFuelingAsync(carId, fuelingId, ct) ? Results.NoContent() : Results.NotFound());

app.MapGet("/api/fuelings", async (ICarTrackerService svc, CancellationToken ct) =>
    Results.Ok(await svc.GetAllFuelingsAsync(ct)));

cars.MapGet("/{carId:guid}/entries", async (Guid carId, ICarTrackerService svc, CancellationToken ct) =>
{
    var items = await svc.GetCarExpenseEntriesAsync(carId, ct);
    return items is null ? Results.NotFound() : Results.Ok(items);
});

cars.MapPost("/{carId:guid}/entries", async (Guid carId, CreateExpenseEntryRequest request, ICarTrackerService svc, CancellationToken ct) =>
{
    try
    {
        var created = await svc.CreateExpenseEntryAsync(carId, request, ct);
        return created is null ? Results.NotFound() : Results.Created($"/api/cars/{carId}/entries/{created.Id}", created);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

cars.MapPatch("/{carId:guid}/entries/{entryId:guid}", async (Guid carId, Guid entryId, UpdateExpenseEntryRequest request, ICarTrackerService svc, CancellationToken ct) =>
{
    try
    {
        var updated = await svc.UpdateExpenseEntryAsync(carId, entryId, request, ct);
        return updated is null ? Results.NotFound() : Results.Ok(updated);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

cars.MapDelete("/{carId:guid}/entries/{entryId:guid}", async (Guid carId, Guid entryId, ICarTrackerService svc, CancellationToken ct) =>
    await svc.DeleteExpenseEntryAsync(carId, entryId, ct) ? Results.NoContent() : Results.NotFound());

cars.MapGet("/{carId:guid}/maintenance-plans", async (Guid carId, ICarTrackerService svc, CancellationToken ct) =>
{
    var items = await svc.GetMaintenancePlansAsync(carId, ct);
    return items is null ? Results.NotFound() : Results.Ok(items);
});

cars.MapPost("/{carId:guid}/maintenance-plans", async (Guid carId, CreateMaintenancePlanItemRequest request, ICarTrackerService svc, CancellationToken ct) =>
{
    try
    {
        var created = await svc.CreateMaintenancePlanItemAsync(carId, request, ct);
        return created is null ? Results.NotFound() : Results.Created($"/api/cars/{carId}/maintenance-plans/{created.Id}", created);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

cars.MapPatch("/{carId:guid}/maintenance-plans/{planId:guid}", async (Guid carId, Guid planId, UpdateMaintenancePlanItemRequest request, ICarTrackerService svc, CancellationToken ct) =>
{
    try
    {
        var updated = await svc.UpdateMaintenancePlanItemAsync(carId, planId, request, ct);
        return updated is null ? Results.NotFound() : Results.Ok(updated);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

cars.MapDelete("/{carId:guid}/maintenance-plans/{planId:guid}", async (Guid carId, Guid planId, ICarTrackerService svc, CancellationToken ct) =>
    await svc.DeleteMaintenancePlanItemAsync(carId, planId, ct) ? Results.NoContent() : Results.NotFound());

cars.MapGet("/{carId:guid}/maintenance-status", async (Guid carId, ICarTrackerService svc, CancellationToken ct) =>
{
    var status = await svc.GetMaintenanceStatusAsync(carId, ct);
    return status is null ? Results.NotFound() : Results.Ok(status);
});

app.MapFallbackToFile("index.html");

app.Run();