using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Entities;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Commands.Fuelings;

public sealed class CreateFuelingCommandHandler(ITrackerPersistence db) : IRequestHandler<CreateFuelingCommand, FuelingEntryDto?>
{
    public async Task<HandlerResult<FuelingEntryDto?>> Handle(CreateFuelingCommand request, CancellationToken cancellationToken)
    {
        var car = await db.GetCarByIdTrackedAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        if (car is null) return RequestOutcome.Ok<FuelingEntryDto?>(null);

        var body = request.Body;

        var entry = new FuelingEntry
        {
            CarId = request.CarId,
            PerformedAt = body.PerformedAt,
            KmAtFueling = body.KmAtFueling,
            Liters = body.Liters,
            TotalPrice = body.TotalPrice,
            FuelType = body.FuelType ?? FuelType.Gasolina,
            IsFullTank = body.IsFullTank ?? false,
            StationName = string.IsNullOrWhiteSpace(body.StationName) ? null : body.StationName.Trim(),
            Notes = string.IsNullOrWhiteSpace(body.Notes) ? null : body.Notes.Trim(),
        };

        db.AddFueling(entry);

        if (entry.KmAtFueling > car.CurrentKm)
            car.CurrentKm = entry.KmAtFueling;

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return RequestOutcome.Ok<FuelingEntryDto?>(new FuelingEntryDto(
            entry.Id,
            entry.CarId,
            entry.PerformedAt,
            entry.KmAtFueling,
            entry.Liters,
            entry.TotalPrice,
            entry.FuelType,
            entry.IsFullTank,
            entry.StationName,
            entry.Notes));
    }
}
