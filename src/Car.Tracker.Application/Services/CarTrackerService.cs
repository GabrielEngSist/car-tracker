using Car.Tracker.Api.Api;
using Car.Tracker.Api.Domain;
using Car.Tracker.Application.Abstractions;
using Car.Tracker.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Car.Tracker.Application.Services;

public sealed class CarTrackerService(IAppDbContext db) : ICarTrackerService
{
    public async Task<IReadOnlyList<CarDto>> GetCarsAsync(CancellationToken cancellationToken = default)
    {
        // SQLite can't ORDER BY DateTimeOffset via EF translation; sort in-memory (cars list is small).
        var carsList = await db.Cars.AsNoTracking().ToListAsync(cancellationToken);
        var items = carsList
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new CarDto(x.Id, x.Model, x.Year, x.CurrentKm, x.Name, x.Placa, x.CreatedAt, x.UpdatedAt))
            .ToList();

        return items;
    }

    public async Task<CarDto?> GetCarAsync(Guid carId, CancellationToken cancellationToken = default)
    {
        var car = await db.Cars.AsNoTracking().FirstOrDefaultAsync(x => x.Id == carId, cancellationToken);
        return car is null ? null : new CarDto(car.Id, car.Model, car.Year, car.CurrentKm, car.Name, car.Placa, car.CreatedAt, car.UpdatedAt);
    }

    public async Task<CarDto> CreateCarAsync(CarEntity car, CancellationToken cancellationToken = default)
    {
        db.Cars.Add(car);
        await db.SaveChangesAsync(cancellationToken);
        return new CarDto(car.Id, car.Model, car.Year, car.CurrentKm, car.Name, car.Placa, car.CreatedAt, car.UpdatedAt);
    }

    public async Task<CarRegistryDto?> GetCarRegistryAsync(Guid carId, CancellationToken cancellationToken = default)
    {
        var car = await db.Cars
            .AsNoTracking()
            .Include(c => c.ConsultaPlaca)
            .Include(c => c.ConsultaPrecoFipe)
            .ThenInclude(f => f!.Itens)
            .FirstOrDefaultAsync(c => c.Id == carId, cancellationToken);

        if (car is null)
            return null;

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

        var carDto = new CarDto(car.Id, car.Model, car.Year, car.CurrentKm, car.Name, car.Placa, car.CreatedAt, car.UpdatedAt);
        return new CarRegistryDto(
            carDto,
            car.ConsultaPlaca is null ? null : ToConsultaPlacaDto(car.ConsultaPlaca),
            car.ConsultaPrecoFipe is null ? null : ToConsultaPrecoFipeDto(car.ConsultaPrecoFipe),
            expenseEntries,
            maintenancePlanItems);
    }

    private static ConsultaPlacaDto ToConsultaPlacaDto(ConsultaPlaca x) =>
        new(
            x.Id,
            x.CarId,
            x.Status,
            x.Mensagem,
            x.DataSolicitacao,
            x.RequestPlaca,
            x.Placa,
            x.Chassi,
            x.AnoFabricacao,
            x.AnoModelo,
            x.Marca,
            x.Modelo,
            x.Cor,
            x.Segmento,
            x.Combustivel,
            x.Procedencia,
            x.Municipio,
            x.UfMunicipio,
            x.TipoVeiculo,
            x.SubSegmento,
            x.NumeroMotor,
            x.NumeroCaixaCambio,
            x.Potencia,
            x.Cilindradas,
            x.NumeroEixos,
            x.CapacidadeMaximaTracao,
            x.CapacidadePassageiro,
            x.CreatedAt,
            x.UpdatedAt);

    private static ConsultaPrecoFipeDto ToConsultaPrecoFipeDto(ConsultaPrecoFipe x)
    {
        var itens = x.Itens
            .OrderBy(i => i.CodigoFipe)
            .ThenBy(i => i.ModeloVersao)
            .Select(i => new ConsultaPrecoFipeItemDto(
                i.Id,
                i.ConsultaPrecoFipeId,
                i.CodigoFipe,
                i.ModeloVersao,
                i.Preco,
                i.MesReferencia,
                i.HistoricoJson,
                i.CreatedAt,
                i.UpdatedAt))
            .ToList();

        return new ConsultaPrecoFipeDto(
            x.Id,
            x.CarId,
            x.Status,
            x.Mensagem,
            x.DataSolicitacao,
            x.RequestPlaca,
            x.VeiculoPlaca,
            x.VeiculoChassi,
            x.VeiculoAnoFabricacao,
            x.VeiculoAnoModelo,
            x.VeiculoMarca,
            x.VeiculoModelo,
            x.VeiculoCor,
            x.VeiculoSegmento,
            x.VeiculoCombustivel,
            x.VeiculoProcedencia,
            x.VeiculoMunicipio,
            x.VeiculoUfMunicipio,
            x.TipoVeiculo,
            x.SubSegmento,
            x.NumeroMotor,
            x.NumeroCaixaCambio,
            x.Potencia,
            x.Cilindradas,
            x.NumeroEixos,
            x.CapacidadeMaximaTracao,
            x.CapacidadePassageiro,
            x.PesoBrutoTotal,
            x.CreatedAt,
            x.UpdatedAt,
            itens);
    }

    public async Task<CarDto?> UpdateCarAsync(Guid carId, UpdateCarRequest request, CancellationToken cancellationToken = default)
    {
        var car = await db.Cars.FirstOrDefaultAsync(x => x.Id == carId, cancellationToken);
        if (car is null) return null;

        if (request.Model is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Model)) throw new ArgumentException("Model cannot be empty.");
            car.Model = request.Model.Trim();
        }

        if (request.Year is not null)
        {
            if (request.Year.Value is < 1900 or > 3000) throw new ArgumentException("Year is invalid.");
            car.Year = request.Year.Value;
        }

        if (request.CurrentKm is not null)
        {
            if (request.CurrentKm.Value < 0) throw new ArgumentException("CurrentKm is invalid.");
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
                    throw new ArgumentException("Invalid plate format.");
                car.Placa = p;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return new CarDto(car.Id, car.Model, car.Year, car.CurrentKm, car.Name, car.Placa, car.CreatedAt, car.UpdatedAt);
    }

    public async Task<bool> DeleteCarAsync(Guid carId, CancellationToken cancellationToken = default)
    {
        var car = await db.Cars.FirstOrDefaultAsync(x => x.Id == carId, cancellationToken);
        if (car is null) return false;
        db.Cars.Remove(car);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<FuelingEntryDto>?> GetCarFuelingsAsync(Guid carId, CancellationToken cancellationToken = default)
    {
        var exists = await db.Cars.AnyAsync(x => x.Id == carId, cancellationToken);
        if (!exists) return null;

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
                x.IsFullTank,
                x.StationName,
                x.Notes))
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<FuelingEntryDto?> CreateFuelingAsync(Guid carId, CreateFuelingEntryRequest request, CancellationToken cancellationToken = default)
    {
        var car = await db.Cars.FirstOrDefaultAsync(x => x.Id == carId, cancellationToken);
        if (car is null) return null;

        if (request.KmAtFueling < 0) throw new ArgumentException("KmAtFueling is invalid.");
        if (request.Liters <= 0) throw new ArgumentException("Liters must be > 0.");
        if (request.TotalPrice < 0) throw new ArgumentException("TotalPrice is invalid.");

        var entry = new FuelingEntry
        {
            CarId = carId,
            PerformedAt = request.PerformedAt,
            KmAtFueling = request.KmAtFueling,
            Liters = request.Liters,
            TotalPrice = request.TotalPrice,
            FuelType = request.FuelType ?? FuelType.Gasolina,
            IsFullTank = request.IsFullTank ?? false,
            StationName = string.IsNullOrWhiteSpace(request.StationName) ? null : request.StationName.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
        };

        db.FuelingEntries.Add(entry);

        if (entry.KmAtFueling > car.CurrentKm)
            car.CurrentKm = entry.KmAtFueling;

        await db.SaveChangesAsync(cancellationToken);

        return new FuelingEntryDto(
            entry.Id,
            entry.CarId,
            entry.PerformedAt,
            entry.KmAtFueling,
            entry.Liters,
            entry.TotalPrice,
            entry.FuelType,
            entry.IsFullTank,
            entry.StationName,
            entry.Notes);
    }

    public async Task<FuelingEntryDto?> UpdateFuelingAsync(Guid carId, Guid fuelingId, UpdateFuelingEntryRequest request, CancellationToken cancellationToken = default)
    {
        var entry = await db.FuelingEntries.FirstOrDefaultAsync(e => e.Id == fuelingId && e.CarId == carId, cancellationToken);
        if (entry is null) return null;

        if (request.PerformedAt is not null) entry.PerformedAt = request.PerformedAt.Value;
        if (request.KmAtFueling is not null)
        {
            if (request.KmAtFueling.Value < 0) throw new ArgumentException("KmAtFueling is invalid.");
            entry.KmAtFueling = request.KmAtFueling.Value;
        }
        if (request.Liters is not null)
        {
            if (request.Liters.Value <= 0) throw new ArgumentException("Liters must be > 0.");
            entry.Liters = request.Liters.Value;
        }
        if (request.TotalPrice is not null)
        {
            if (request.TotalPrice.Value < 0) throw new ArgumentException("TotalPrice is invalid.");
            entry.TotalPrice = request.TotalPrice.Value;
        }
        if (request.FuelType is not null)
            entry.FuelType = request.FuelType.Value;
        if (request.IsFullTank is not null)
            entry.IsFullTank = request.IsFullTank.Value;
        if (request.StationName is not null)
            entry.StationName = string.IsNullOrWhiteSpace(request.StationName) ? null : request.StationName.Trim();
        if (request.Notes is not null)
            entry.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await db.SaveChangesAsync(cancellationToken);

        return new FuelingEntryDto(
            entry.Id,
            entry.CarId,
            entry.PerformedAt,
            entry.KmAtFueling,
            entry.Liters,
            entry.TotalPrice,
            entry.FuelType,
            entry.IsFullTank,
            entry.StationName,
            entry.Notes);
    }

    public async Task<bool> DeleteFuelingAsync(Guid carId, Guid fuelingId, CancellationToken cancellationToken = default)
    {
        var entry = await db.FuelingEntries.FirstOrDefaultAsync(e => e.Id == fuelingId && e.CarId == carId, cancellationToken);
        if (entry is null) return false;
        db.FuelingEntries.Remove(entry);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<FuelingEntryDto>> GetAllFuelingsAsync(CancellationToken cancellationToken = default)
    {
        return await db.FuelingEntries
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
                x.IsFullTank,
                x.StationName,
                x.Notes))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExpenseEntryDto>?> GetCarExpenseEntriesAsync(Guid carId, CancellationToken cancellationToken = default)
    {
        var exists = await db.Cars.AnyAsync(x => x.Id == carId, cancellationToken);
        if (!exists) return null;

        return await db.ExpenseEntries
            .Where(x => x.CarId == carId)
            .OrderByDescending(x => x.PerformedAt)
            .ThenByDescending(x => x.KmAtService)
            .Select(x => new ExpenseEntryDto(
                x.Id, x.CarId, x.Type, x.Title, x.Price, x.SupplierBrand, x.ProductModel, x.PerformedAt, x.KmAtService, x.Notes))
            .ToListAsync(cancellationToken);
    }

    public async Task<ExpenseEntryDto?> CreateExpenseEntryAsync(Guid carId, CreateExpenseEntryRequest request, CancellationToken cancellationToken = default)
    {
        var car = await db.Cars.FirstOrDefaultAsync(x => x.Id == carId, cancellationToken);
        if (car is null) return null;

        if (string.IsNullOrWhiteSpace(request.Title)) throw new ArgumentException("Title is required.");
        if (request.Price < 0) throw new ArgumentException("Price is invalid.");
        if (request.KmAtService < 0) throw new ArgumentException("KmAtService is invalid.");

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
        await db.SaveChangesAsync(cancellationToken);

        return new ExpenseEntryDto(
            entry.Id, entry.CarId, entry.Type, entry.Title, entry.Price, entry.SupplierBrand, entry.ProductModel, entry.PerformedAt, entry.KmAtService, entry.Notes);
    }

    public async Task<ExpenseEntryDto?> UpdateExpenseEntryAsync(Guid carId, Guid entryId, UpdateExpenseEntryRequest request, CancellationToken cancellationToken = default)
    {
        var entry = await db.ExpenseEntries.FirstOrDefaultAsync(e => e.Id == entryId && e.CarId == carId, cancellationToken);
        if (entry is null) return null;

        if (request.Type is not null) entry.Type = request.Type.Value;
        if (request.Title is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Title)) throw new ArgumentException("Title cannot be empty.");
            entry.Title = request.Title.Trim();
        }
        if (request.Price is not null)
        {
            if (request.Price.Value < 0) throw new ArgumentException("Price is invalid.");
            entry.Price = request.Price.Value;
        }
        if (request.SupplierBrand is not null)
            entry.SupplierBrand = string.IsNullOrWhiteSpace(request.SupplierBrand) ? null : request.SupplierBrand.Trim();
        if (request.ProductModel is not null)
            entry.ProductModel = string.IsNullOrWhiteSpace(request.ProductModel) ? null : request.ProductModel.Trim();
        if (request.PerformedAt is not null) entry.PerformedAt = request.PerformedAt.Value;
        if (request.KmAtService is not null)
        {
            if (request.KmAtService.Value < 0) throw new ArgumentException("KmAtService is invalid.");
            entry.KmAtService = request.KmAtService.Value;
        }
        if (request.Notes is not null)
            entry.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await db.SaveChangesAsync(cancellationToken);
        return new ExpenseEntryDto(
            entry.Id, entry.CarId, entry.Type, entry.Title, entry.Price, entry.SupplierBrand, entry.ProductModel, entry.PerformedAt, entry.KmAtService, entry.Notes);
    }

    public async Task<bool> DeleteExpenseEntryAsync(Guid carId, Guid entryId, CancellationToken cancellationToken = default)
    {
        var entry = await db.ExpenseEntries.FirstOrDefaultAsync(e => e.Id == entryId && e.CarId == carId, cancellationToken);
        if (entry is null) return false;
        db.ExpenseEntries.Remove(entry);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<MaintenancePlanItemDto>?> GetMaintenancePlansAsync(Guid carId, CancellationToken cancellationToken = default)
    {
        var exists = await db.Cars.AnyAsync(x => x.Id == carId, cancellationToken);
        if (!exists) return null;

        return await db.MaintenancePlanItems
            .Where(x => x.CarId == carId)
            .OrderByDescending(x => x.Active)
            .ThenBy(x => x.Title)
            .Select(x => new MaintenancePlanItemDto(x.Id, x.CarId, x.Title, x.DueKmInterval, x.DueTimeIntervalDays, x.Active))
            .ToListAsync(cancellationToken);
    }

    public async Task<MaintenancePlanItemDto?> CreateMaintenancePlanItemAsync(Guid carId, CreateMaintenancePlanItemRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await db.Cars.AnyAsync(x => x.Id == carId, cancellationToken);
        if (!exists) return null;

        if (string.IsNullOrWhiteSpace(request.Title)) throw new ArgumentException("Title is required.");
        if (request.DueKmInterval is null && request.DueTimeIntervalDays is null)
            throw new ArgumentException("At least one interval is required (km or days).");
        if (request.DueKmInterval is not null && request.DueKmInterval <= 0) throw new ArgumentException("DueKmInterval must be > 0.");
        if (request.DueTimeIntervalDays is not null && request.DueTimeIntervalDays <= 0) throw new ArgumentException("DueTimeIntervalDays must be > 0.");

        var item = new MaintenancePlanItem
        {
            CarId = carId,
            Title = request.Title.Trim(),
            DueKmInterval = request.DueKmInterval,
            DueTimeIntervalDays = request.DueTimeIntervalDays,
            Active = request.Active,
        };

        db.MaintenancePlanItems.Add(item);
        await db.SaveChangesAsync(cancellationToken);

        return new MaintenancePlanItemDto(item.Id, item.CarId, item.Title, item.DueKmInterval, item.DueTimeIntervalDays, item.Active);
    }

    public async Task<MaintenancePlanItemDto?> UpdateMaintenancePlanItemAsync(Guid carId, Guid planId, UpdateMaintenancePlanItemRequest request, CancellationToken cancellationToken = default)
    {
        var item = await db.MaintenancePlanItems.FirstOrDefaultAsync(x => x.Id == planId && x.CarId == carId, cancellationToken);
        if (item is null) return null;

        if (request.Title is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Title)) throw new ArgumentException("Title cannot be empty.");
            item.Title = request.Title.Trim();
        }
        if (request.DueKmInterval is not null)
        {
            if (request.DueKmInterval <= 0) throw new ArgumentException("DueKmInterval must be > 0.");
            item.DueKmInterval = request.DueKmInterval;
        }
        if (request.DueTimeIntervalDays is not null)
        {
            if (request.DueTimeIntervalDays <= 0) throw new ArgumentException("DueTimeIntervalDays must be > 0.");
            item.DueTimeIntervalDays = request.DueTimeIntervalDays;
        }
        if (request.Active is not null) item.Active = request.Active.Value;

        var km = item.DueKmInterval;
        var days = item.DueTimeIntervalDays;
        if (km is null && days is null)
            throw new ArgumentException("At least one interval is required (km or days).");

        await db.SaveChangesAsync(cancellationToken);
        return new MaintenancePlanItemDto(item.Id, item.CarId, item.Title, item.DueKmInterval, item.DueTimeIntervalDays, item.Active);
    }

    public async Task<bool> DeleteMaintenancePlanItemAsync(Guid carId, Guid planId, CancellationToken cancellationToken = default)
    {
        var item = await db.MaintenancePlanItems.FirstOrDefaultAsync(x => x.Id == planId && x.CarId == carId, cancellationToken);
        if (item is null) return false;
        db.MaintenancePlanItems.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<MaintenanceStatusDto>?> GetMaintenanceStatusAsync(Guid carId, CancellationToken cancellationToken = default)
    {
        var car = await db.Cars.AsNoTracking().FirstOrDefaultAsync(x => x.Id == carId, cancellationToken);
        if (car is null) return null;

        var plans = await db.MaintenancePlanItems
            .Where(x => x.CarId == carId && x.Active)
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken);

        var serviceEntries = await db.ExpenseEntries
            .Where(x => x.CarId == carId && x.Type == ExpenseEntryType.Service)
            .OrderByDescending(x => x.PerformedAt)
            .ThenByDescending(x => x.KmAtService)
            .ToListAsync(cancellationToken);

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

        result = result
            .OrderByDescending(x => x.Overdue)
            .ThenBy(x =>
            {
                var km = x.NextDueKm;
                var date = x.NextDueDate;
                var kmValue = km ?? int.MaxValue;
                var dateValue = date ?? DateOnly.MaxValue;
                var primary = dateValue.ToDateTime(TimeOnly.MinValue);
                var secondary = kmValue;
                return (primary, secondary);
            })
            .ToList();

        return result;
    }

    public async Task<FuelFullTankEfficiencyReportDto?> GetFuelFullTankReportAsync(
        Guid carId,
        bool lifetimeMode,
        PeriodAggregator periodAgg,
        DateOnly today,
        CancellationToken cancellationToken = default)
    {
        var car = await db.Cars.AsNoTracking().FirstOrDefaultAsync(c => c.Id == carId, cancellationToken);
        if (car is null) return null;

        var fuels = await db.FuelingEntries.AsNoTracking()
            .Where(f => f.CarId == carId)
            .ToListAsync(cancellationToken);

        return lifetimeMode
            ? FuelFullTankEfficiencyCalculator.Compute(carId, car, fuels, lifetimeMode: true, PeriodAggregator.Total, today)
            : FuelFullTankEfficiencyCalculator.Compute(carId, car, fuels, lifetimeMode: false, periodAgg, today);
    }

    public async Task<CostPerKmReportDto?> GetCostPerKmReportAsync(
        Guid carId,
        bool lifetimeMode,
        PeriodAggregator periodAgg,
        DistanceReferenceMultiplier distanceMultiplier,
        DateOnly today,
        CancellationToken cancellationToken = default)
    {
        var car = await db.Cars.AsNoTracking().FirstOrDefaultAsync(c => c.Id == carId, cancellationToken);
        if (car is null) return null;

        var expenses = await db.ExpenseEntries.AsNoTracking()
            .Where(e => e.CarId == carId)
            .ToListAsync(cancellationToken);

        var fuels = await db.FuelingEntries.AsNoTracking()
            .Where(f => f.CarId == carId)
            .ToListAsync(cancellationToken);

        return lifetimeMode
            ? CostPerKmReportCalculator.ComputeByDistanceLifetime(carId, car, expenses, fuels, distanceMultiplier, today)
            : CostPerKmReportCalculator.ComputeByPeriod(carId, car, expenses, fuels, periodAgg, today, distanceMultiplier);
    }
}

