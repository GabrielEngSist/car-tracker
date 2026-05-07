using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Queries.Cars;

public sealed class GetCarByIdQueryHandler(ITrackerPersistence db) : IRequestHandler<GetCarByIdQuery, CarDto?>
{
    public async Task<HandlerResult<CarDto?>> Handle(GetCarByIdQuery request, CancellationToken cancellationToken)
    {
        var car = await db.GetCarByIdReadOnlyAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        return RequestOutcome.Ok(car is null ? null : new CarDto(car.Id, car.Model, car.Year, car.CurrentKm, car.Name, car.Placa, car.CreatedAt, car.UpdatedAt));
    }
}
