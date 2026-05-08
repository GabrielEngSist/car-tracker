using Car.Tracker.Domain.Entities;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Tests.TestDoubles;

public sealed class FakeTrackerPersistence : ITrackerPersistence
{
    public List<CarEntity> Cars { get; } = [];
    public List<FuelingEntry> Fuelings { get; } = [];
    public List<ExpenseEntry> ExpenseEntries { get; } = [];
    public List<MaintenancePlanItem> MaintenancePlans { get; } = [];

    public int SaveChangesCallCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.FromResult(1);
    }

    public Task<IReadOnlyList<CarEntity>> ListCarsOrderedByCreatedDescendingAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<CarEntity>>(Cars.OrderByDescending(c => c.CreatedAt).ToList());

    public Task<CarEntity?> GetCarByIdReadOnlyAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Cars.FirstOrDefault(c => c.Id == id));

    public Task<CarEntity?> GetCarByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Cars.FirstOrDefault(c => c.Id == id));

    public Task<CarEntity?> GetCarWithConsultasForRegistryAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Cars.FirstOrDefault(c => c.Id == id));

    public Task<bool> CarExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Cars.Any(c => c.Id == id));

    public void AddCar(CarEntity car)
    {
        if (car.Id == Guid.Empty)
            car.Id = Guid.NewGuid();
        if (car.CreatedAt == default)
            car.CreatedAt = DateTimeOffset.UtcNow;
        car.UpdatedAt = car.CreatedAt;
        Cars.Add(car);
    }

    public void RemoveCar(CarEntity car) => Cars.Remove(car);

    public Task<IReadOnlyList<ExpenseEntry>> GetExpenseEntriesForRegistryAsync(Guid carId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ExpenseEntry>>(ExpenseEntries.Where(e => e.CarId == carId).OrderByDescending(e => e.PerformedAt).ToList());

    public Task<IReadOnlyList<MaintenancePlanItem>> GetMaintenancePlanItemsForRegistryAsync(Guid carId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<MaintenancePlanItem>>(MaintenancePlans.Where(p => p.CarId == carId).OrderBy(p => p.Title).ToList());

    public Task<IReadOnlyList<MaintenancePlanItem>> GetMaintenancePlanItemsForPlansEndpointAsync(Guid carId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<MaintenancePlanItem>>(
            MaintenancePlans
                .Where(p => p.CarId == carId)
                .OrderByDescending(p => p.Active)
                .ThenBy(p => p.Title)
                .ToList());

    public Task<IReadOnlyList<FuelingEntry>> GetFuelingsForCarOrderedAsync(Guid carId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<FuelingEntry>>(
            Fuelings.Where(f => f.CarId == carId).OrderByDescending(f => f.PerformedAt).ToList());

    public Task<IReadOnlyList<FuelingEntry>> GetAllFuelingsOrderedAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<FuelingEntry>>(Fuelings.OrderByDescending(f => f.PerformedAt).ToList());

    public Task<FuelingEntry?> GetFuelingByCarAndIdTrackedAsync(Guid carId, Guid fuelingId, CancellationToken cancellationToken = default)
        => Task.FromResult(Fuelings.FirstOrDefault(f => f.CarId == carId && f.Id == fuelingId));

    public void AddFueling(FuelingEntry entry)
    {
        if (entry.Id == Guid.Empty)
            entry.Id = Guid.NewGuid();
        Fuelings.Add(entry);
    }

    public void RemoveFueling(FuelingEntry entry) => Fuelings.Remove(entry);

    public Task<ExpenseEntry?> GetExpenseEntryByCarAndIdTrackedAsync(Guid carId, Guid entryId, CancellationToken cancellationToken = default)
        => Task.FromResult(ExpenseEntries.FirstOrDefault(e => e.CarId == carId && e.Id == entryId));

    public void AddExpenseEntry(ExpenseEntry entry)
    {
        if (entry.Id == Guid.Empty)
            entry.Id = Guid.NewGuid();
        ExpenseEntries.Add(entry);
    }

    public void RemoveExpenseEntry(ExpenseEntry entry) => ExpenseEntries.Remove(entry);

    public Task<MaintenancePlanItem?> GetMaintenancePlanItemByCarAndIdTrackedAsync(Guid carId, Guid planId, CancellationToken cancellationToken = default)
        => Task.FromResult(MaintenancePlans.FirstOrDefault(p => p.CarId == carId && p.Id == planId));

    public void AddMaintenancePlanItem(MaintenancePlanItem item)
    {
        if (item.Id == Guid.Empty)
            item.Id = Guid.NewGuid();
        MaintenancePlans.Add(item);
    }

    public void RemoveMaintenancePlanItem(MaintenancePlanItem item) => MaintenancePlans.Remove(item);

    public Task<IReadOnlyList<ExpenseEntry>> GetExpenseEntriesForReportsAsync(Guid carId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ExpenseEntry>>(ExpenseEntries.Where(e => e.CarId == carId).OrderBy(e => e.PerformedAt).ToList());

    public Task<IReadOnlyList<FuelingEntry>> GetFuelingsForReportsAsync(Guid carId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<FuelingEntry>>(Fuelings.Where(f => f.CarId == carId).OrderBy(f => f.PerformedAt).ToList());

    public Task<IReadOnlyList<MaintenancePlanItem>> GetActiveMaintenancePlansOrderedAsync(Guid carId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<MaintenancePlanItem>>(
            MaintenancePlans.Where(p => p.CarId == carId && p.Active).OrderBy(p => p.Title).ToList());

    public Task<IReadOnlyList<ExpenseEntry>> GetServiceExpenseEntriesForMaintenanceStatusAsync(Guid carId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ExpenseEntry>>(
            ExpenseEntries
                .Where(e => e.CarId == carId && e.Type == ExpenseEntryType.Service)
                .OrderByDescending(e => e.PerformedAt)
                .ToList());
}

