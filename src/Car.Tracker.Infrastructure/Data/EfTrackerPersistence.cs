using Car.Tracker.Domain.Entities;
using Car.Tracker.Domain.Ports.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Car.Tracker.Infrastructure.Data;

/// <summary>EF Core adapter for <see cref="ITrackerPersistence"/> (replaceable with another store implementation).</summary>
public sealed class EfTrackerPersistence(AppDbContext db) : ITrackerPersistence
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);

    public async Task<IReadOnlyList<CarEntity>> ListCarsOrderedByCreatedDescendingAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.Cars
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<CarEntity?> GetCarByIdReadOnlyAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Cars.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<CarEntity?> GetCarByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Cars.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<CarEntity?> GetCarWithConsultasForRegistryAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await db.Cars
            .AsNoTracking()
            .Include(c => c.ConsultaPlaca)
            .Include(c => c.ConsultaPrecoFipe)
            .ThenInclude(f => f!.Itens)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<bool> CarExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Cars.AnyAsync(x => x.Id == id, cancellationToken);

    public void AddCar(CarEntity car) => db.Cars.Add(car);

    public void RemoveCar(CarEntity car) => db.Cars.Remove(car);

    public async Task<IReadOnlyList<ExpenseEntry>> GetExpenseEntriesForRegistryAsync(
        Guid carId,
        CancellationToken cancellationToken = default)
    {
        return await db.ExpenseEntries
            .AsNoTracking()
            .Where(x => x.CarId == carId)
            .OrderByDescending(x => x.PerformedAt)
            .ThenByDescending(x => x.KmAtService)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<MaintenancePlanItem>> GetMaintenancePlanItemsForRegistryAsync(
        Guid carId,
        CancellationToken cancellationToken = default)
    {
        return await db.MaintenancePlanItems
            .AsNoTracking()
            .Where(x => x.CarId == carId)
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<MaintenancePlanItem>> GetMaintenancePlanItemsForPlansEndpointAsync(
        Guid carId,
        CancellationToken cancellationToken = default)
    {
        return await db.MaintenancePlanItems
            .AsNoTracking()
            .Where(x => x.CarId == carId)
            .OrderByDescending(x => x.Active)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<FuelingEntry>> GetFuelingsForCarOrderedAsync(
        Guid carId,
        CancellationToken cancellationToken = default)
    {
        return await db.FuelingEntries
            .AsNoTracking()
            .Where(x => x.CarId == carId)
            .OrderByDescending(x => x.PerformedAt)
            .ThenByDescending(x => x.KmAtFueling)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<FuelingEntry>> GetAllFuelingsOrderedAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.FuelingEntries
            .AsNoTracking()
            .OrderByDescending(x => x.PerformedAt)
            .ThenByDescending(x => x.KmAtFueling)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<FuelingEntry?> GetFuelingByCarAndIdTrackedAsync(
        Guid carId,
        Guid fuelingId,
        CancellationToken cancellationToken = default) =>
        db.FuelingEntries.FirstOrDefaultAsync(
            e => e.Id == fuelingId && e.CarId == carId,
            cancellationToken);

    public void AddFueling(FuelingEntry entry) => db.FuelingEntries.Add(entry);

    public void RemoveFueling(FuelingEntry entry) => db.FuelingEntries.Remove(entry);

    public Task<ExpenseEntry?> GetExpenseEntryByCarAndIdTrackedAsync(
        Guid carId,
        Guid entryId,
        CancellationToken cancellationToken = default) =>
        db.ExpenseEntries.FirstOrDefaultAsync(
            e => e.Id == entryId && e.CarId == carId,
            cancellationToken);

    public void AddExpenseEntry(ExpenseEntry entry) => db.ExpenseEntries.Add(entry);

    public void RemoveExpenseEntry(ExpenseEntry entry) => db.ExpenseEntries.Remove(entry);

    public Task<MaintenancePlanItem?> GetMaintenancePlanItemByCarAndIdTrackedAsync(
        Guid carId,
        Guid planId,
        CancellationToken cancellationToken = default) =>
        db.MaintenancePlanItems.FirstOrDefaultAsync(
            x => x.Id == planId && x.CarId == carId,
            cancellationToken);

    public void AddMaintenancePlanItem(MaintenancePlanItem item) => db.MaintenancePlanItems.Add(item);

    public void RemoveMaintenancePlanItem(MaintenancePlanItem item) => db.MaintenancePlanItems.Remove(item);

    public async Task<IReadOnlyList<ExpenseEntry>> GetExpenseEntriesForReportsAsync(
        Guid carId,
        CancellationToken cancellationToken = default)
    {
        return await db.ExpenseEntries
            .AsNoTracking()
            .Where(e => e.CarId == carId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<FuelingEntry>> GetFuelingsForReportsAsync(
        Guid carId,
        CancellationToken cancellationToken = default)
    {
        return await db.FuelingEntries
            .AsNoTracking()
            .Where(f => f.CarId == carId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<MaintenancePlanItem>> GetActiveMaintenancePlansOrderedAsync(
        Guid carId,
        CancellationToken cancellationToken = default)
    {
        return await db.MaintenancePlanItems
            .AsNoTracking()
            .Where(x => x.CarId == carId && x.Active)
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ExpenseEntry>> GetServiceExpenseEntriesForMaintenanceStatusAsync(
        Guid carId,
        CancellationToken cancellationToken = default)
    {
        return await db.ExpenseEntries
            .AsNoTracking()
            .Where(x => x.CarId == carId && x.Type == ExpenseEntryType.Service)
            .OrderByDescending(x => x.PerformedAt)
            .ThenByDescending(x => x.KmAtService)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
