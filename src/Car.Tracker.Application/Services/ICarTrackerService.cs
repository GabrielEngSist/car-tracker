using Car.Tracker.Domain.Reports;
using Car.Tracker.Domain.Entities;
using Car.Tracker.Contracts;

namespace Car.Tracker.Application.Services;

public interface ICarTrackerService
{
    Task<IReadOnlyList<CarDto>> GetCarsAsync(CancellationToken cancellationToken = default);
    Task<CarDto?> GetCarAsync(Guid carId, CancellationToken cancellationToken = default);

    Task<CarDto> CreateCarAsync(CarEntity car, CancellationToken cancellationToken = default);
    Task<CarRegistryDto?> GetCarRegistryAsync(Guid carId, CancellationToken cancellationToken = default);
    Task<CarDto?> UpdateCarAsync(Guid carId, UpdateCarRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteCarAsync(Guid carId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FuelingEntryDto>?> GetCarFuelingsAsync(Guid carId, CancellationToken cancellationToken = default);
    Task<FuelingEntryDto?> CreateFuelingAsync(Guid carId, CreateFuelingEntryRequest request, CancellationToken cancellationToken = default);
    Task<FuelingEntryDto?> UpdateFuelingAsync(Guid carId, Guid fuelingId, UpdateFuelingEntryRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteFuelingAsync(Guid carId, Guid fuelingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FuelingEntryDto>> GetAllFuelingsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExpenseEntryDto>?> GetCarExpenseEntriesAsync(Guid carId, CancellationToken cancellationToken = default);
    Task<ExpenseEntryDto?> CreateExpenseEntryAsync(Guid carId, CreateExpenseEntryRequest request, CancellationToken cancellationToken = default);
    Task<ExpenseEntryDto?> UpdateExpenseEntryAsync(Guid carId, Guid entryId, UpdateExpenseEntryRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteExpenseEntryAsync(Guid carId, Guid entryId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MaintenancePlanItemDto>?> GetMaintenancePlansAsync(Guid carId, CancellationToken cancellationToken = default);
    Task<MaintenancePlanItemDto?> CreateMaintenancePlanItemAsync(Guid carId, CreateMaintenancePlanItemRequest request, CancellationToken cancellationToken = default);
    Task<MaintenancePlanItemDto?> UpdateMaintenancePlanItemAsync(Guid carId, Guid planId, UpdateMaintenancePlanItemRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteMaintenancePlanItemAsync(Guid carId, Guid planId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MaintenanceStatusDto>?> GetMaintenanceStatusAsync(Guid carId, CancellationToken cancellationToken = default);

    Task<FuelFullTankEfficiencyReportDto?> GetFuelFullTankReportAsync(Guid carId, bool lifetimeMode, PeriodAggregator periodAgg, DateOnly today, CancellationToken cancellationToken = default);
    Task<CostPerKmReportDto?> GetCostPerKmReportAsync(Guid carId, bool lifetimeMode, PeriodAggregator periodAgg, DistanceReferenceMultiplier distanceMultiplier, DateOnly today, CancellationToken cancellationToken = default);
}

