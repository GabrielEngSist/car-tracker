using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Plates;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Commands.Cars;

public sealed class UpdateCarCommandHandler(ITrackerPersistence db) : IRequestHandler<UpdateCarCommand, CarDto?>
{
    public async Task<HandlerResult<CarDto?>> Handle(UpdateCarCommand request, CancellationToken cancellationToken)
    {
        var car = await db.GetCarByIdTrackedAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        if (car is null) return RequestOutcome.Ok<CarDto?>(null);

        var r = request.Request;
        if (r.Model is not null)
        {
            if (string.IsNullOrWhiteSpace(r.Model))
                return RequestOutcome.Fail<CarDto?>("MODEL_EMPTY", "Model cannot be empty.", nameof(r.Model));
            car.Model = r.Model.Trim();
        }

        if (r.Year is not null)
        {
            if (r.Year.Value is < 1900 or > 3000)
                return RequestOutcome.Fail<CarDto?>("YEAR_INVALID", "Year is invalid.", nameof(r.Year));
            car.Year = r.Year.Value;
        }

        if (r.CurrentKm is not null)
        {
            if (r.CurrentKm.Value < 0)
                return RequestOutcome.Fail<CarDto?>("CURRENT_KM_INVALID", "CurrentKm is invalid.", nameof(r.CurrentKm));
            car.CurrentKm = r.CurrentKm.Value;
        }

        if (r.Name is not null)
            car.Name = string.IsNullOrWhiteSpace(r.Name) ? null : r.Name.Trim();

        if (r.Placa is not null)
        {
            if (string.IsNullOrWhiteSpace(r.Placa))
                car.Placa = null;
            else
            {
                var p = PlacaBrasil.Normalizar(r.Placa);
                if (!PlacaBrasil.EhValida(p))
                    return RequestOutcome.Fail<CarDto?>("PLATE_INVALID", "Invalid plate format.", nameof(r.Placa));
                car.Placa = p;
            }
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return RequestOutcome.Ok<CarDto?>(new CarDto(car.Id, car.Model, car.Year, car.CurrentKm, car.Name, car.Placa, car.CreatedAt, car.UpdatedAt));
    }
}
