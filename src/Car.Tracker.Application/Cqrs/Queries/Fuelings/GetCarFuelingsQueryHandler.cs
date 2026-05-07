using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Queries.Fuelings;

public sealed class GetCarFuelingsQueryHandler(ITrackerPersistence db) : IRequestHandler<GetCarFuelingsQuery, IReadOnlyList<FuelingEntryDto>?>
{
    public async Task<HandlerResult<IReadOnlyList<FuelingEntryDto>?>> Handle(GetCarFuelingsQuery request, CancellationToken cancellationToken)
    {
        var exists = await db.CarExistsAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        if (!exists) return RequestOutcome.Ok<IReadOnlyList<FuelingEntryDto>?>(null);

        var list = await db.GetFuelingsForCarOrderedAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        var dtos = list
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
            .ToList();
        return RequestOutcome.Ok<IReadOnlyList<FuelingEntryDto>?>(dtos);
    }
}
