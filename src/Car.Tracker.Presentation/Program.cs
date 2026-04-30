using Car.Tracker.Presentation.Api;
using Car.Tracker.Presentation.ConsultarPlacaModels;
using Car.Tracker.Presentation.Data;
using Car.Tracker.Presentation.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.Net.Http;
using System.Text.Json.Serialization;
using Car.Tracker.Presentation.Configuration;

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

builder.Services.AddDbContext<AppDbContext>((svc, options) =>
{
    var settings = svc.GetRequiredService<IOptions<DatabaseSettings>>().Value;
    var connectionString = $"Host={settings.Host};Database={settings.Database};Username={settings.Username};Password={settings.Password};Port={settings.Port};SSL Mode=VerifyFull;Channel Binding=Require";
    options.UseNpgsql(connectionString);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDeveloperExceptionPage();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }))
    .WithName("Health");

var cars = app.MapGroup("/api/cars");

cars.MapGet("/", async (AppDbContext db) =>
{
    // SQLite can't ORDER BY DateTimeOffset via EF translation; sort in-memory (cars list is small).
    var carsList = await db.Cars.AsNoTracking().ToListAsync();
    var items = carsList
        .OrderByDescending(x => x.CreatedAt)
        .Select(x => new CarDto(x.Id, x.Model, x.Year, x.CurrentKm, x.Name, x.Placa, x.CreatedAt, x.UpdatedAt))
        .ToList();

    return Results.Ok(items);
});

cars.MapPost("/", async (
    CreateCarRequest request,
    AppDbContext db,
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

        db.Cars.Add(car);
        await db.SaveChangesAsync();

        return Results.Created($"/api/cars/{car.Id}", new CarDto(car.Id, car.Model, car.Year, car.CurrentKm, car.Name, car.Placa, car.CreatedAt, car.UpdatedAt));
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

    db.Cars.Add(carManual);
    await db.SaveChangesAsync();

    return Results.Created($"/api/cars/{carManual.Id}", new CarDto(carManual.Id, carManual.Model, carManual.Year, carManual.CurrentKm, carManual.Name, carManual.Placa, carManual.CreatedAt, carManual.UpdatedAt));
});

cars.MapGet("/{carId:guid}", async (Guid carId, AppDbContext db) =>
{
    var car = await db.Cars.FindAsync(carId);
    if (car is null) return Results.NotFound();
    return Results.Ok(new CarDto(car.Id, car.Model, car.Year, car.CurrentKm, car.Name, car.Placa, car.CreatedAt, car.UpdatedAt));
});

cars.MapGet("/{carId:guid}/registry", async (Guid carId, AppDbContext db, CancellationToken cancellationToken) =>
{
    var car = await db.Cars
        .AsNoTracking()
        .Include(c => c.ConsultaPlaca)
        .Include(c => c.ConsultaPrecoFipe)
        .ThenInclude(f => f!.Itens)
        .FirstOrDefaultAsync(c => c.Id == carId, cancellationToken);

    if (car is null)
        return Results.NotFound();

    var expenseEntries = await db.ExpenseEntries
        .AsNoTracking()
        .Where(x => x.CarId == carId)
        .OrderByDescending(x => x.PerformedAt)
        .ThenByDescending(x => x.KmAtService)
        .Select(x => new ExpenseEntryDto(
            x.Id, x.CarId, x.Type, x.Title, x.Price, x.SupplierBrand, x.ProductModel, x.PerformedAt, x.KmAtService, x.Notes))
        .ToListAsync(cancellationToken);

    var maintenancePlanItems = await db.MaintenancePlanItems
        .AsNoTracking()
        .Where(x => x.CarId == carId)
        .OrderBy(x => x.Title)
        .Select(x => new MaintenancePlanItemDto(x.Id, x.CarId, x.Title, x.DueKmInterval, x.DueTimeIntervalDays, x.Active))
        .ToListAsync(cancellationToken);

    return Results.Ok(CarRegistryMapper.ToRegistry(car, expenseEntries, maintenancePlanItems));
});

cars.MapPatch("/{carId:guid}", async (Guid carId, UpdateCarRequest request, AppDbContext db) =>
{
    var car = await db.Cars.FindAsync(carId);
    if (car is null) return Results.NotFound();

    if (request.Model is not null)
    {
        if (string.IsNullOrWhiteSpace(request.Model)) return Results.BadRequest("Model cannot be empty.");
        car.Model = request.Model.Trim();
    }

    if (request.Year is not null)
    {
        if (request.Year.Value is < 1900 or > 3000) return Results.BadRequest("Year is invalid.");
        car.Year = request.Year.Value;
    }

    if (request.CurrentKm is not null)
    {
        if (request.CurrentKm.Value < 0) return Results.BadRequest("CurrentKm is invalid.");
        car.CurrentKm = request.CurrentKm.Value;
    }

    if (request.Name is not null)
    {
        car.Name = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name.Trim();
    }

    if (request.Placa is not null)
    {
        if (string.IsNullOrWhiteSpace(request.Placa))
            car.Placa = null;
        else
        {
            var p = PlacaBrasil.Normalizar(request.Placa);
            if (!PlacaBrasil.EhValida(p))
                return Results.BadRequest("Invalid plate format.");
            car.Placa = p;
        }
    }

    await db.SaveChangesAsync();
    return Results.Ok(new CarDto(car.Id, car.Model, car.Year, car.CurrentKm, car.Name, car.Placa, car.CreatedAt, car.UpdatedAt));
});

cars.MapDelete("/{carId:guid}", async (Guid carId, AppDbContext db) =>
{
    var car = await db.Cars.FindAsync(carId);
    if (car is null) return Results.NotFound();

    db.Cars.Remove(car);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

cars.MapGet("/{carId:guid}/fuelings", async (Guid carId, AppDbContext db) =>
{
    var exists = await db.Cars.AnyAsync(x => x.Id == carId);
    if (!exists) return Results.NotFound();

    var items = await db.FuelingEntries
        .Where(x => x.CarId == carId)
        .OrderByDescending(x => x.PerformedAt)
        .ThenByDescending(x => x.KmAtFueling)
        .Select(x => new FuelingEntryDto(
            x.Id,
            x.CarId,
            x.PerformedAt,
            x.KmAtFueling,
            x.Liters,
            x.TotalPrice,
            x.FuelType,
            x.StationName,
            x.Notes))
        .ToListAsync();

    return Results.Ok(items);
});

cars.MapGet("/{carId:guid}/reports/cost-per-km", async (
    Guid carId,
    string? basis,
    string? period,
    string? distanceRef,
    AppDbContext db) =>
{
    var car = await db.Cars.AsNoTracking().FirstOrDefaultAsync(c => c.Id == carId);
    if (car is null) return Results.NotFound();

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

    var expenses = await db.ExpenseEntries.AsNoTracking()
        .Where(e => e.CarId == carId)
        .ToListAsync();

    var fuels = await db.FuelingEntries.AsNoTracking()
        .Where(f => f.CarId == carId)
        .ToListAsync();

    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    var report = lifetimeMode
        ? CostPerKmReportCalculator.ComputeByDistanceLifetime(carId, car, expenses, fuels, dMult, today)
        : CostPerKmReportCalculator.ComputeByPeriod(carId, car, expenses, fuels, periodAgg, today, dMult);

    return Results.Ok(report);
});

cars.MapPost("/{carId:guid}/fuelings", async (Guid carId, CreateFuelingEntryRequest request, AppDbContext db) =>
{
    var car = await db.Cars.FindAsync(carId);
    if (car is null) return Results.NotFound();

    if (request.KmAtFueling < 0) return Results.BadRequest("KmAtFueling is invalid.");
    if (request.Liters <= 0) return Results.BadRequest("Liters must be > 0.");
    if (request.TotalPrice < 0) return Results.BadRequest("TotalPrice is invalid.");

    var entry = new FuelingEntry
    {
        CarId = carId,
        PerformedAt = request.PerformedAt,
        KmAtFueling = request.KmAtFueling,
        Liters = request.Liters,
        TotalPrice = request.TotalPrice,
        FuelType = request.FuelType ?? FuelType.Gasolina,
        StationName = string.IsNullOrWhiteSpace(request.StationName) ? null : request.StationName.Trim(),
        Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
    };

    db.FuelingEntries.Add(entry);

    // Atualiza o hodômetro se o abastecimento for mais recente.
    if (entry.KmAtFueling > car.CurrentKm)
        car.CurrentKm = entry.KmAtFueling;

    await db.SaveChangesAsync();

    return Results.Created($"/api/cars/{carId}/fuelings/{entry.Id}", new FuelingEntryDto(
        entry.Id,
        entry.CarId,
        entry.PerformedAt,
        entry.KmAtFueling,
        entry.Liters,
        entry.TotalPrice,
        entry.FuelType,
        entry.StationName,
        entry.Notes));
});

cars.MapPatch("/{carId:guid}/fuelings/{fuelingId:guid}", async (Guid carId, Guid fuelingId, UpdateFuelingEntryRequest request, AppDbContext db) =>
{
    var entry = await db.FuelingEntries.FirstOrDefaultAsync(e => e.Id == fuelingId && e.CarId == carId);
    if (entry is null) return Results.NotFound();

    if (request.PerformedAt is not null) entry.PerformedAt = request.PerformedAt.Value;
    if (request.KmAtFueling is not null)
    {
        if (request.KmAtFueling.Value < 0) return Results.BadRequest("KmAtFueling is invalid.");
        entry.KmAtFueling = request.KmAtFueling.Value;
    }
    if (request.Liters is not null)
    {
        if (request.Liters.Value <= 0) return Results.BadRequest("Liters must be > 0.");
        entry.Liters = request.Liters.Value;
    }
    if (request.TotalPrice is not null)
    {
        if (request.TotalPrice.Value < 0) return Results.BadRequest("TotalPrice is invalid.");
        entry.TotalPrice = request.TotalPrice.Value;
    }
    if (request.FuelType is not null)
        entry.FuelType = request.FuelType.Value;
    if (request.StationName is not null)
        entry.StationName = string.IsNullOrWhiteSpace(request.StationName) ? null : request.StationName.Trim();
    if (request.Notes is not null)
        entry.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

    await db.SaveChangesAsync();

    return Results.Ok(new FuelingEntryDto(
        entry.Id,
        entry.CarId,
        entry.PerformedAt,
        entry.KmAtFueling,
        entry.Liters,
        entry.TotalPrice,
        entry.FuelType,
        entry.StationName,
        entry.Notes));
});

cars.MapDelete("/{carId:guid}/fuelings/{fuelingId:guid}", async (Guid carId, Guid fuelingId, AppDbContext db) =>
{
    var entry = await db.FuelingEntries.FirstOrDefaultAsync(e => e.Id == fuelingId && e.CarId == carId);
    if (entry is null) return Results.NotFound();
    db.FuelingEntries.Remove(entry);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapGet("/api/fuelings", async (AppDbContext db) =>
{
    var items = await db.FuelingEntries
        .OrderByDescending(x => x.PerformedAt)
        .ThenByDescending(x => x.KmAtFueling)
        .Select(x => new FuelingEntryDto(
            x.Id,
            x.CarId,
            x.PerformedAt,
            x.KmAtFueling,
            x.Liters,
            x.TotalPrice,
            x.FuelType,
            x.StationName,
            x.Notes))
        .ToListAsync();

    return Results.Ok(items);
});

cars.MapGet("/{carId:guid}/entries", async (Guid carId, AppDbContext db) =>
{
    var exists = await db.Cars.AnyAsync(x => x.Id == carId);
    if (!exists) return Results.NotFound();

    var items = await db.ExpenseEntries
        .Where(x => x.CarId == carId)
        .OrderByDescending(x => x.PerformedAt)
        .ThenByDescending(x => x.KmAtService)
        .Select(x => new ExpenseEntryDto(
            x.Id, x.CarId, x.Type, x.Title, x.Price, x.SupplierBrand, x.ProductModel, x.PerformedAt, x.KmAtService, x.Notes))
        .ToListAsync();

    return Results.Ok(items);
});

cars.MapPost("/{carId:guid}/entries", async (Guid carId, CreateExpenseEntryRequest request, AppDbContext db) =>
{
    var car = await db.Cars.FindAsync(carId);
    if (car is null) return Results.NotFound();

    if (string.IsNullOrWhiteSpace(request.Title)) return Results.BadRequest("Title is required.");
    if (request.Price < 0) return Results.BadRequest("Price is invalid.");
    if (request.KmAtService < 0) return Results.BadRequest("KmAtService is invalid.");

    var entry = new ExpenseEntry
    {
        CarId = carId,
        Type = request.Type,
        Title = request.Title.Trim(),
        Price = request.Price,
        SupplierBrand = string.IsNullOrWhiteSpace(request.SupplierBrand) ? null : request.SupplierBrand.Trim(),
        ProductModel = string.IsNullOrWhiteSpace(request.ProductModel) ? null : request.ProductModel.Trim(),
        PerformedAt = request.PerformedAt,
        KmAtService = request.KmAtService,
        Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
    };

    db.ExpenseEntries.Add(entry);
    await db.SaveChangesAsync();

    return Results.Created($"/api/cars/{carId}/entries/{entry.Id}", new ExpenseEntryDto(
        entry.Id, entry.CarId, entry.Type, entry.Title, entry.Price, entry.SupplierBrand, entry.ProductModel, entry.PerformedAt, entry.KmAtService, entry.Notes));
});

cars.MapPatch("/{carId:guid}/entries/{entryId:guid}", async (Guid carId, Guid entryId, UpdateExpenseEntryRequest request, AppDbContext db) =>
{
    var entry = await db.ExpenseEntries.FirstOrDefaultAsync(e => e.Id == entryId && e.CarId == carId);
    if (entry is null) return Results.NotFound();

    if (request.Type is not null) entry.Type = request.Type.Value;
    if (request.Title is not null)
    {
        if (string.IsNullOrWhiteSpace(request.Title)) return Results.BadRequest("Title cannot be empty.");
        entry.Title = request.Title.Trim();
    }
    if (request.Price is not null)
    {
        if (request.Price.Value < 0) return Results.BadRequest("Price is invalid.");
        entry.Price = request.Price.Value;
    }
    if (request.SupplierBrand is not null)
        entry.SupplierBrand = string.IsNullOrWhiteSpace(request.SupplierBrand) ? null : request.SupplierBrand.Trim();
    if (request.ProductModel is not null)
        entry.ProductModel = string.IsNullOrWhiteSpace(request.ProductModel) ? null : request.ProductModel.Trim();
    if (request.PerformedAt is not null) entry.PerformedAt = request.PerformedAt.Value;
    if (request.KmAtService is not null)
    {
        if (request.KmAtService.Value < 0) return Results.BadRequest("KmAtService is invalid.");
        entry.KmAtService = request.KmAtService.Value;
    }
    if (request.Notes is not null)
        entry.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

    await db.SaveChangesAsync();
    return Results.Ok(new ExpenseEntryDto(
        entry.Id, entry.CarId, entry.Type, entry.Title, entry.Price, entry.SupplierBrand, entry.ProductModel, entry.PerformedAt, entry.KmAtService, entry.Notes));
});

cars.MapDelete("/{carId:guid}/entries/{entryId:guid}", async (Guid carId, Guid entryId, AppDbContext db) =>
{
    var entry = await db.ExpenseEntries.FirstOrDefaultAsync(e => e.Id == entryId && e.CarId == carId);
    if (entry is null) return Results.NotFound();
    db.ExpenseEntries.Remove(entry);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

cars.MapGet("/{carId:guid}/maintenance-plans", async (Guid carId, AppDbContext db) =>
{
    var exists = await db.Cars.AnyAsync(x => x.Id == carId);
    if (!exists) return Results.NotFound();

    var items = await db.MaintenancePlanItems
        .Where(x => x.CarId == carId)
        .OrderByDescending(x => x.Active)
        .ThenBy(x => x.Title)
        .Select(x => new MaintenancePlanItemDto(x.Id, x.CarId, x.Title, x.DueKmInterval, x.DueTimeIntervalDays, x.Active))
        .ToListAsync();

    return Results.Ok(items);
});

cars.MapPost("/{carId:guid}/maintenance-plans", async (Guid carId, CreateMaintenancePlanItemRequest request, AppDbContext db) =>
{
    var exists = await db.Cars.AnyAsync(x => x.Id == carId);
    if (!exists) return Results.NotFound();

    if (string.IsNullOrWhiteSpace(request.Title)) return Results.BadRequest("Title is required.");
    if (request.DueKmInterval is null && request.DueTimeIntervalDays is null)
        return Results.BadRequest("At least one interval is required (km or days).");
    if (request.DueKmInterval is not null && request.DueKmInterval <= 0) return Results.BadRequest("DueKmInterval must be > 0.");
    if (request.DueTimeIntervalDays is not null && request.DueTimeIntervalDays <= 0) return Results.BadRequest("DueTimeIntervalDays must be > 0.");

    var item = new MaintenancePlanItem
    {
        CarId = carId,
        Title = request.Title.Trim(),
        DueKmInterval = request.DueKmInterval,
        DueTimeIntervalDays = request.DueTimeIntervalDays,
        Active = request.Active,
    };

    db.MaintenancePlanItems.Add(item);
    await db.SaveChangesAsync();

    return Results.Created($"/api/cars/{carId}/maintenance-plans/{item.Id}",
        new MaintenancePlanItemDto(item.Id, item.CarId, item.Title, item.DueKmInterval, item.DueTimeIntervalDays, item.Active));
});

cars.MapPatch("/{carId:guid}/maintenance-plans/{planId:guid}", async (Guid carId, Guid planId, UpdateMaintenancePlanItemRequest request, AppDbContext db) =>
{
    var item = await db.MaintenancePlanItems.FirstOrDefaultAsync(x => x.Id == planId && x.CarId == carId);
    if (item is null) return Results.NotFound();

    if (request.Title is not null)
    {
        if (string.IsNullOrWhiteSpace(request.Title)) return Results.BadRequest("Title cannot be empty.");
        item.Title = request.Title.Trim();
    }
    if (request.DueKmInterval is not null)
    {
        if (request.DueKmInterval <= 0) return Results.BadRequest("DueKmInterval must be > 0.");
        item.DueKmInterval = request.DueKmInterval;
    }
    if (request.DueTimeIntervalDays is not null)
    {
        if (request.DueTimeIntervalDays <= 0) return Results.BadRequest("DueTimeIntervalDays must be > 0.");
        item.DueTimeIntervalDays = request.DueTimeIntervalDays;
    }
    if (request.Active is not null) item.Active = request.Active.Value;

    var km = item.DueKmInterval;
    var days = item.DueTimeIntervalDays;
    if (km is null && days is null)
        return Results.BadRequest("At least one interval is required (km or days).");

    await db.SaveChangesAsync();
    return Results.Ok(new MaintenancePlanItemDto(item.Id, item.CarId, item.Title, item.DueKmInterval, item.DueTimeIntervalDays, item.Active));
});

cars.MapDelete("/{carId:guid}/maintenance-plans/{planId:guid}", async (Guid carId, Guid planId, AppDbContext db) =>
{
    var item = await db.MaintenancePlanItems.FirstOrDefaultAsync(x => x.Id == planId && x.CarId == carId);
    if (item is null) return Results.NotFound();
    db.MaintenancePlanItems.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

cars.MapGet("/{carId:guid}/maintenance-status", async (Guid carId, AppDbContext db) =>
{
    var car = await db.Cars.FindAsync(carId);
    if (car is null) return Results.NotFound();

    var plans = await db.MaintenancePlanItems
        .Where(x => x.CarId == carId && x.Active)
        .OrderBy(x => x.Title)
        .ToListAsync();

    var serviceEntries = await db.ExpenseEntries
        .Where(x => x.CarId == carId && x.Type == ExpenseEntryType.Service)
        .OrderByDescending(x => x.PerformedAt)
        .ThenByDescending(x => x.KmAtService)
        .ToListAsync();

    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    var result = plans.Select(plan =>
    {
        var last = serviceEntries.FirstOrDefault(e =>
            string.Equals(e.Title, plan.Title, StringComparison.OrdinalIgnoreCase));

        var baselineDate = last?.PerformedAt;
        var baselineKm = last?.KmAtService;

        DateOnly? nextDueDate = null;
        int? nextDueKm = null;

        if (plan.DueTimeIntervalDays is not null)
        {
            var start = baselineDate ?? DateOnly.FromDateTime(car.CreatedAt.UtcDateTime);
            nextDueDate = start.AddDays(plan.DueTimeIntervalDays.Value);
        }

        if (plan.DueKmInterval is not null)
        {
            var start = baselineKm ?? car.CurrentKm;
            nextDueKm = start + plan.DueKmInterval.Value;
        }

        var overdueByTime = nextDueDate is not null && nextDueDate.Value <= today;
        var overdueByKm = nextDueKm is not null && nextDueKm.Value <= car.CurrentKm;

        var overdue = overdueByTime || overdueByKm;

        return new MaintenanceStatusDto(
            plan.Id,
            plan.Title,
            plan.DueKmInterval,
            plan.DueTimeIntervalDays,
            baselineDate,
            baselineKm,
            nextDueDate,
            nextDueKm,
            overdueByTime,
            overdueByKm,
            overdue);
    }).ToList();

    // Sort by whichever comes first (date or km), overdue first
    result = result
        .OrderByDescending(x => x.Overdue)
        .ThenBy(x =>
        {
            var km = x.NextDueKm;
            var date = x.NextDueDate;
            // Normalize missing values as far future
            var kmValue = km ?? int.MaxValue;
            var dateValue = date ?? DateOnly.MaxValue;
            // Compare primarily by date, then km
            var primary = dateValue.ToDateTime(TimeOnly.MinValue);
            var secondary = kmValue;
            return (primary, secondary);
        })
        .ToList();

    return Results.Ok(result);
});

app.MapFallbackToFile("index.html");

app.Run();