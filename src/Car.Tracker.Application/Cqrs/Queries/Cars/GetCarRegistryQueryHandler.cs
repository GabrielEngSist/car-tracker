using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Queries.Cars;

public sealed class GetCarRegistryQueryHandler(ITrackerPersistence db) : IRequestHandler<GetCarRegistryQuery, CarRegistryDto?>
{
    public async Task<CarRegistryDto?> Handle(GetCarRegistryQuery request, CancellationToken cancellationToken)
    {
        var car = await db.GetCarWithConsultasForRegistryAsync(request.CarId, cancellationToken).ConfigureAwait(false);

        if (car is null)
            return null;

        var expenseEntriesRaw = await db.GetExpenseEntriesForRegistryAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        var expenseEntries = expenseEntriesRaw
            .Select(x => new ExpenseEntryDto(
                x.Id, x.CarId, x.Type, x.Title, x.Price, x.SupplierBrand, x.ProductModel, x.PerformedAt, x.KmAtService, x.Notes))
            .ToList();

        var maintenancePlanItemsRaw = await db.GetMaintenancePlanItemsForRegistryAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        var maintenancePlanItems = maintenancePlanItemsRaw
            .Select(x => new MaintenancePlanItemDto(x.Id, x.CarId, x.Title, x.DueKmInterval, x.DueTimeIntervalDays, x.Active))
            .ToList();

        var carDto = new CarDto(car.Id, car.Model, car.Year, car.CurrentKm, car.Name, car.Placa, car.CreatedAt, car.UpdatedAt);
        return new CarRegistryDto(
            carDto,
            car.ConsultaPlaca is null ? null : CarRegistryDtoMapping.ToConsultaPlacaDto(car.ConsultaPlaca),
            car.ConsultaPrecoFipe is null ? null : CarRegistryDtoMapping.ToConsultaPrecoFipeDto(car.ConsultaPrecoFipe),
            expenseEntries,
            maintenancePlanItems);
    }
}
