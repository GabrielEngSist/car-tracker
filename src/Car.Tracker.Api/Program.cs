using Car.Tracker.Api;
using Car.Tracker.Api.Configuration;
using Car.Tracker.Application;
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
{
    var r = await mediator.SendAsync(new GetCarsQuery(), ct);
    return r.IsFailure ? MediatorHttp.ValidationProblem(r) : Results.Ok(r.Value);
});

cars.MapPost("/", async (CreateCarRequest request, IMediator mediator, CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new CreateCarCommand(request), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    var outcome = r.Value!;
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
    var r = await mediator.SendAsync(new GetCarByIdQuery(carId), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    var car = r.Value;
    return car is null ? Results.NotFound() : Results.Ok(car);
});

cars.MapGet("/{carId:guid}/registry", async (Guid carId, IMediator mediator, CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new GetCarRegistryQuery(carId), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    var registry = r.Value;
    return registry is null ? Results.NotFound() : Results.Ok(registry);
});

cars.MapPatch("/{carId:guid}", async (Guid carId, UpdateCarRequest request, IMediator mediator, CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new UpdateCarCommand(carId, request), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    var updated = r.Value;
    return updated is null ? Results.NotFound() : Results.Ok(updated);
});

cars.MapDelete("/{carId:guid}", async (Guid carId, IMediator mediator, CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new DeleteCarCommand(carId), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    return r.Value! ? Results.NoContent() : Results.NotFound();
});

cars.MapGet("/{carId:guid}/fuelings", async (Guid carId, IMediator mediator, CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new GetCarFuelingsQuery(carId), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    var items = r.Value;
    return items is null ? Results.NotFound() : Results.Ok(items);
});

cars.MapGet("/{carId:guid}/reports/fuel-full-tank", async (
    Guid carId,
    string? basis,
    string? period,
    IMediator mediator,
    CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new GetFuelFullTankReportQuery(carId, basis, period), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    var report = r.Value;
    return report is null ? Results.NotFound() : Results.Ok(report);
});

cars.MapGet("/{carId:guid}/reports/cost-per-km", async (
    Guid carId,
    string? basis,
    string? period,
    string? distanceRef,
    IMediator mediator,
    CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new GetCostPerKmReportQuery(carId, basis, period, distanceRef), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    var report = r.Value;
    return report is null ? Results.NotFound() : Results.Ok(report);
});

cars.MapPost("/{carId:guid}/fuelings", async (Guid carId, CreateFuelingEntryRequest request, IMediator mediator, CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new CreateFuelingCommand(carId, request), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    var created = r.Value;
    return created is null ? Results.NotFound() : Results.Created($"/api/cars/{carId}/fuelings/{created.Id}", created);
});

cars.MapPatch("/{carId:guid}/fuelings/{fuelingId:guid}", async (Guid carId, Guid fuelingId, UpdateFuelingEntryRequest request, IMediator mediator, CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new UpdateFuelingCommand(carId, fuelingId, request), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    var updated = r.Value;
    return updated is null ? Results.NotFound() : Results.Ok(updated);
});

cars.MapDelete("/{carId:guid}/fuelings/{fuelingId:guid}", async (Guid carId, Guid fuelingId, IMediator mediator, CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new DeleteFuelingCommand(carId, fuelingId), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    return r.Value! ? Results.NoContent() : Results.NotFound();
});

app.MapGet("/api/fuelings", async (IMediator mediator, CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new GetAllFuelingsQuery(), ct);
    return r.IsFailure ? MediatorHttp.ValidationProblem(r) : Results.Ok(r.Value);
});

cars.MapGet("/{carId:guid}/entries", async (Guid carId, IMediator mediator, CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new GetCarExpenseEntriesQuery(carId), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    var items = r.Value;
    return items is null ? Results.NotFound() : Results.Ok(items);
});

cars.MapPost("/{carId:guid}/entries", async (Guid carId, CreateExpenseEntryRequest request, IMediator mediator, CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new CreateExpenseEntryCommand(carId, request), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    var created = r.Value;
    return created is null ? Results.NotFound() : Results.Created($"/api/cars/{carId}/entries/{created.Id}", created);
});

cars.MapPatch("/{carId:guid}/entries/{entryId:guid}", async (Guid carId, Guid entryId, UpdateExpenseEntryRequest request, IMediator mediator, CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new UpdateExpenseEntryCommand(carId, entryId, request), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    var updated = r.Value;
    return updated is null ? Results.NotFound() : Results.Ok(updated);
});

cars.MapDelete("/{carId:guid}/entries/{entryId:guid}", async (Guid carId, Guid entryId, IMediator mediator, CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new DeleteExpenseEntryCommand(carId, entryId), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    return r.Value! ? Results.NoContent() : Results.NotFound();
});

cars.MapGet("/{carId:guid}/maintenance-plans", async (Guid carId, IMediator mediator, CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new GetMaintenancePlansQuery(carId), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    var items = r.Value;
    return items is null ? Results.NotFound() : Results.Ok(items);
});

cars.MapPost("/{carId:guid}/maintenance-plans", async (Guid carId, CreateMaintenancePlanItemRequest request, IMediator mediator, CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new CreateMaintenancePlanItemCommand(carId, request), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    var created = r.Value;
    return created is null ? Results.NotFound() : Results.Created($"/api/cars/{carId}/maintenance-plans/{created.Id}", created);
});

cars.MapPatch("/{carId:guid}/maintenance-plans/{planId:guid}", async (Guid carId, Guid planId, UpdateMaintenancePlanItemRequest request, IMediator mediator, CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new UpdateMaintenancePlanItemCommand(carId, planId, request), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    var updated = r.Value;
    return updated is null ? Results.NotFound() : Results.Ok(updated);
});

cars.MapDelete("/{carId:guid}/maintenance-plans/{planId:guid}", async (Guid carId, Guid planId, IMediator mediator, CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new DeleteMaintenancePlanItemCommand(carId, planId), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    return r.Value! ? Results.NoContent() : Results.NotFound();
});

cars.MapGet("/{carId:guid}/maintenance-status", async (Guid carId, IMediator mediator, CancellationToken ct) =>
{
    var r = await mediator.SendAsync(new GetMaintenanceStatusQuery(carId), ct);
    if (r.IsFailure)
        return MediatorHttp.ValidationProblem(r);
    var status = r.Value;
    return status is null ? Results.NotFound() : Results.Ok(status);
});

app.MapFallbackToFile("index.html");

app.Run();
