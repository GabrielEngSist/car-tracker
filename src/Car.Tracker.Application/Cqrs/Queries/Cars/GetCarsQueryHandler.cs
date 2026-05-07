using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Queries.Cars;

public sealed class GetCarsQueryHandler(ITrackerPersistence db) : IRequestHandler<GetCarsQuery, IReadOnlyList<CarDto>>
{
    public async Task<IReadOnlyList<CarDto>> Handle(GetCarsQuery request, CancellationToken cancellationToken)
    {
        var carsList = await db.ListCarsOrderedByCreatedDescendingAsync(cancellationToken).ConfigureAwait(false);
        return carsList
            .Select(x => new CarDto(x.Id, x.Model, x.Year, x.CurrentKm, x.Name, x.Placa, x.CreatedAt, x.UpdatedAt))
            .ToList();
    }
}
