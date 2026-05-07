using Car.Tracker.Application.Common;
using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Plates;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Commands.Cars;

public sealed class UpdateCarCommandHandler(ITrackerPersistence db) : IRequestHandler<UpdateCarCommand, CarDto?>
{
    public async Task<CarDto?> Handle(UpdateCarCommand request, CancellationToken cancellationToken)
    {
        var car = await db.GetCarByIdTrackedAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        if (car is null) return null;

        var r = request.Request;
        if (r.Model is not null)
        {
            if (string.IsNullOrWhiteSpace(r.Model)) throw new ValidationException("Model cannot be empty.");
            car.Model = r.Model.Trim();
        }

        if (r.Year is not null)
        {
            if (r.Year.Value is < 1900 or > 3000) throw new ValidationException("Year is invalid.");
            car.Year = r.Year.Value;
        }

        if (r.CurrentKm is not null)
        {
            if (r.CurrentKm.Value < 0) throw new ValidationException("CurrentKm is invalid.");
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
                    throw new ValidationException("Invalid plate format.");
                car.Placa = p;
            }
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new CarDto(car.Id, car.Model, car.Year, car.CurrentKm, car.Name, car.Placa, car.CreatedAt, car.UpdatedAt);
    }
}
