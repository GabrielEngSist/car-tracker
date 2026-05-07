using Car.Tracker.Domain.Entities;

namespace Car.Tracker.Domain.Ports.Persistence;

/// <summary>
/// Outbound persistence port: application and domain use cases depend on this abstraction,
/// not on EF, PostgreSQL, or any concrete storage technology.
/// </summary>
public interface ITrackerPersistence
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CarEntity>> ListCarsOrderedByCreatedDescendingAsync(
        CancellationToken cancellationToken = default);

    Task<CarEntity?> GetCarByIdReadOnlyAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CarEntity?> GetCarByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Car with optional one-to-one consulta relations (and FIPE line items) for registry API.</summary>
    Task<CarEntity?> GetCarWithConsultasForRegistryAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> CarExistsAsync(Guid id, CancellationToken cancellationToken = default);

    void AddCar(CarEntity car);
    void RemoveCar(CarEntity car);

    Task<IReadOnlyList<ExpenseEntry>> GetExpenseEntriesForRegistryAsync(
        Guid carId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MaintenancePlanItem>> GetMaintenancePlanItemsForRegistryAsync(
        Guid carId,
        CancellationToken cancellationToken = default);

    /// <summary>Maintenance plans for the car’s plan list API (active first, then title).</summary>
    Task<IReadOnlyList<MaintenancePlanItem>> GetMaintenancePlanItemsForPlansEndpointAsync(
        Guid carId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FuelingEntry>> GetFuelingsForCarOrderedAsync(
        Guid carId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FuelingEntry>> GetAllFuelingsOrderedAsync(CancellationToken cancellationToken = default);

    Task<FuelingEntry?> GetFuelingByCarAndIdTrackedAsync(
        Guid carId,
        Guid fuelingId,
        CancellationToken cancellationToken = default);

    void AddFueling(FuelingEntry entry);
    void RemoveFueling(FuelingEntry entry);

    Task<ExpenseEntry?> GetExpenseEntryByCarAndIdTrackedAsync(
        Guid carId,
        Guid entryId,
        CancellationToken cancellationToken = default);

    void AddExpenseEntry(ExpenseEntry entry);
    void RemoveExpenseEntry(ExpenseEntry entry);

    Task<MaintenancePlanItem?> GetMaintenancePlanItemByCarAndIdTrackedAsync(
        Guid carId,
        Guid planId,
        CancellationToken cancellationToken = default);

    void AddMaintenancePlanItem(MaintenancePlanItem item);
    void RemoveMaintenancePlanItem(MaintenancePlanItem item);

    Task<IReadOnlyList<ExpenseEntry>> GetExpenseEntriesForReportsAsync(
        Guid carId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FuelingEntry>> GetFuelingsForReportsAsync(
        Guid carId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MaintenancePlanItem>> GetActiveMaintenancePlansOrderedAsync(
        Guid carId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExpenseEntry>> GetServiceExpenseEntriesForMaintenanceStatusAsync(
        Guid carId,
        CancellationToken cancellationToken = default);
}
