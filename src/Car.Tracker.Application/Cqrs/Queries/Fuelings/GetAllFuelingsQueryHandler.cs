using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Queries.Fuelings;

public sealed class GetAllFuelingsQueryHandler(ITrackerPersistence db) : IRequestHandler<GetAllFuelingsQuery, IReadOnlyList<FuelingEntryDto>>
{
    public async Task<HandlerResult<IReadOnlyList<FuelingEntryDto>>> Handle(GetAllFuelingsQuery request, CancellationToken cancellationToken)
    {
        var list = await db.GetAllFuelingsOrderedAsync(cancellationToken).ConfigureAwait(false);
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
        return RequestOutcome.Ok<IReadOnlyList<FuelingEntryDto>>(dtos);
    }
}
