using Car.Tracker.Api.Configuration;
using Car.Tracker.Application;
using Car.Tracker.Application.Common;
using Car.Tracker.Application.Cqrs.Commands.Cars;
using Car.Tracker.Application.Cqrs.Commands.ExpenseEntries;
using Car.Tracker.Application.Cqrs.Commands.Fuelings;
using Car.Tracker.Application.Cqrs.Commands.Maintenance;
using Car.Tracker.Application.Cqrs.Queries.Cars;
using Car.Tracker.Application.Cqrs.Queries.ExpenseEntries;
using Car.Tracker.Application.Cqrs.Queries.Fuelings;
using Car.Tracker.Application.Cqrs.Queries.Maintenance;
using Car.Tracker.Application.Cqrs.Queries.Reports;
using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Infrastructure;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var dbSettings = builder.Configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>() ?? new DatabaseSettings();
var connectionString = $"Host={dbSettings.Host};Database={dbSettings.Database};Username={dbSettings.Username};Password={dbSettings.Password};Port={dbSettings.Port};SSL Mode=VerifyFull;Channel Binding=Require";

builder.Services.AddConsultarPlacaIntegration(builder.Configuration);
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplication();

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

cars.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
    Results.Ok(await mediator.Send(new GetCarsQuery(), ct)));

cars.MapPost("/", async (CreateCarRequest request, IMediator mediator, CancellationToken ct) =>
{
    var outcome = await mediator.Send(new CreateCarCommand(request), ct);
    return outcome.Status switch
    {
        CreateCarStatus.Created => Results.Created($"/api/cars/{outcome.Car!.Id}", outcome.Car),
        CreateCarStatus.BadRequest => Results.BadRequest(outcome.Message),
        CreateCarStatus.BadGateway => Results.Problem(detail: outcome.Message, statusCode: StatusCodes.Status502BadGateway),
        _ => Results.Problem(statusCode: 500),
    };
});

cars.MapGet("/{carId:guid}", async (Guid carId, IMediator mediator, CancellationToken ct) =>
{
    var car = await mediator.Send(new GetCarByIdQuery(carId), ct);
    return car is null ? Results.NotFound() : Results.Ok(car);
});

cars.MapGet("/{carId:guid}/registry", async (Guid carId, IMediator mediator, CancellationToken ct) =>
{
    var registry = await mediator.Send(new GetCarRegistryQuery(carId), ct);
    return registry is null ? Results.NotFound() : Results.Ok(registry);
});

cars.MapPatch("/{carId:guid}", async (Guid carId, UpdateCarRequest request, IMediator mediator, CancellationToken ct) =>
{
    try
    {
        var updated = await mediator.Send(new UpdateCarCommand(carId, request), ct);
        return updated is null ? Results.NotFound() : Results.Ok(updated);
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

cars.MapDelete("/{carId:guid}", async (Guid carId, IMediator mediator, CancellationToken ct) =>
    await mediator.Send(new DeleteCarCommand(carId), ct) ? Results.NoContent() : Results.NotFound());

cars.MapGet("/{carId:guid}/fuelings", async (Guid carId, IMediator mediator, CancellationToken ct) =>
{
    var items = await mediator.Send(new GetCarFuelingsQuery(carId), ct);
    return items is null ? Results.NotFound() : Results.Ok(items);
});

cars.MapGet("/{carId:guid}/reports/fuel-full-tank", async (
    Guid carId,
    string? basis,
    string? period,
    IMediator mediator,
    CancellationToken ct) =>
{
    try
    {
        var report = await mediator.Send(new GetFuelFullTankReportQuery(carId, basis, period), ct);
        return report is null ? Results.NotFound() : Results.Ok(report);
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

cars.MapGet("/{carId:guid}/reports/cost-per-km", async (
    Guid carId,
    string? basis,
    string? period,
    string? distanceRef,
    IMediator mediator,
    CancellationToken ct) =>
{
    try
    {
        var report = await mediator.Send(new GetCostPerKmReportQuery(carId, basis, period, distanceRef), ct);
        return report is null ? Results.NotFound() : Results.Ok(report);
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

cars.MapPost("/{carId:guid}/fuelings", async (Guid carId, CreateFuelingEntryRequest request, IMediator mediator, CancellationToken ct) =>
{
    try
    {
        var created = await mediator.Send(new CreateFuelingCommand(carId, request), ct);
        return created is null ? Results.NotFound() : Results.Created($"/api/cars/{carId}/fuelings/{created.Id}", created);
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

cars.MapPatch("/{carId:guid}/fuelings/{fuelingId:guid}", async (Guid carId, Guid fuelingId, UpdateFuelingEntryRequest request, IMediator mediator, CancellationToken ct) =>
{
    try
    {
        var updated = await mediator.Send(new UpdateFuelingCommand(carId, fuelingId, request), ct);
        return updated is null ? Results.NotFound() : Results.Ok(updated);
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

cars.MapDelete("/{carId:guid}/fuelings/{fuelingId:guid}", async (Guid carId, Guid fuelingId, IMediator mediator, CancellationToken ct) =>
    await mediator.Send(new DeleteFuelingCommand(carId, fuelingId), ct) ? Results.NoContent() : Results.NotFound());

app.MapGet("/api/fuelings", async (IMediator mediator, CancellationToken ct) =>
    Results.Ok(await mediator.Send(new GetAllFuelingsQuery(), ct)));

cars.MapGet("/{carId:guid}/entries", async (Guid carId, IMediator mediator, CancellationToken ct) =>
{
    var items = await mediator.Send(new GetCarExpenseEntriesQuery(carId), ct);
    return items is null ? Results.NotFound() : Results.Ok(items);
});

cars.MapPost("/{carId:guid}/entries", async (Guid carId, CreateExpenseEntryRequest request, IMediator mediator, CancellationToken ct) =>
{
    try
    {
        var created = await mediator.Send(new CreateExpenseEntryCommand(carId, request), ct);
        return created is null ? Results.NotFound() : Results.Created($"/api/cars/{carId}/entries/{created.Id}", created);
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

cars.MapPatch("/{carId:guid}/entries/{entryId:guid}", async (Guid carId, Guid entryId, UpdateExpenseEntryRequest request, IMediator mediator, CancellationToken ct) =>
{
    try
    {
        var updated = await mediator.Send(new UpdateExpenseEntryCommand(carId, entryId, request), ct);
        return updated is null ? Results.NotFound() : Results.Ok(updated);
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

cars.MapDelete("/{carId:guid}/entries/{entryId:guid}", async (Guid carId, Guid entryId, IMediator mediator, CancellationToken ct) =>
    await mediator.Send(new DeleteExpenseEntryCommand(carId, entryId), ct) ? Results.NoContent() : Results.NotFound());

cars.MapGet("/{carId:guid}/maintenance-plans", async (Guid carId, IMediator mediator, CancellationToken ct) =>
{
    var items = await mediator.Send(new GetMaintenancePlansQuery(carId), ct);
    return items is null ? Results.NotFound() : Results.Ok(items);
});

cars.MapPost("/{carId:guid}/maintenance-plans", async (Guid carId, CreateMaintenancePlanItemRequest request, IMediator mediator, CancellationToken ct) =>
{
    try
    {
        var created = await mediator.Send(new CreateMaintenancePlanItemCommand(carId, request), ct);
        return created is null ? Results.NotFound() : Results.Created($"/api/cars/{carId}/maintenance-plans/{created.Id}", created);
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

cars.MapPatch("/{carId:guid}/maintenance-plans/{planId:guid}", async (Guid carId, Guid planId, UpdateMaintenancePlanItemRequest request, IMediator mediator, CancellationToken ct) =>
{
    try
    {
        var updated = await mediator.Send(new UpdateMaintenancePlanItemCommand(carId, planId, request), ct);
        return updated is null ? Results.NotFound() : Results.Ok(updated);
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

cars.MapDelete("/{carId:guid}/maintenance-plans/{planId:guid}", async (Guid carId, Guid planId, IMediator mediator, CancellationToken ct) =>
    await mediator.Send(new DeleteMaintenancePlanItemCommand(carId, planId), ct) ? Results.NoContent() : Results.NotFound());

cars.MapGet("/{carId:guid}/maintenance-status", async (Guid carId, IMediator mediator, CancellationToken ct) =>
{
    var status = await mediator.Send(new GetMaintenanceStatusQuery(carId), ct);
    return status is null ? Results.NotFound() : Results.Ok(status);
});

app.MapFallbackToFile("index.html");

app.Run();
