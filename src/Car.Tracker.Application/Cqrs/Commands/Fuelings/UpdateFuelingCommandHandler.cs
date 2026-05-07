using Car.Tracker.Application.Common;
using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Commands.Fuelings;

public sealed class UpdateFuelingCommandHandler(ITrackerPersistence db) : IRequestHandler<UpdateFuelingCommand, FuelingEntryDto?>
{
    public async Task<FuelingEntryDto?> Handle(UpdateFuelingCommand request, CancellationToken cancellationToken)
    {
        var entry = await db.GetFuelingByCarAndIdTrackedAsync(request.CarId, request.FuelingId, cancellationToken).ConfigureAwait(false);
        if (entry is null) return null;

        var body = request.Body;
        if (body.PerformedAt is not null) entry.PerformedAt = body.PerformedAt.Value;
        if (body.KmAtFueling is not null)
        {
            if (body.KmAtFueling.Value < 0) throw new ValidationException("KmAtFueling is invalid.");
            entry.KmAtFueling = body.KmAtFueling.Value;
        }
        if (body.Liters is not null)
        {
            if (body.Liters.Value <= 0) throw new ValidationException("Liters must be > 0.");
            entry.Liters = body.Liters.Value;
        }
        if (body.TotalPrice is not null)
        {
            if (body.TotalPrice.Value < 0) throw new ValidationException("TotalPrice is invalid.");
            entry.TotalPrice = body.TotalPrice.Value;
        }
        if (body.FuelType is not null)
            entry.FuelType = body.FuelType.Value;
        if (body.IsFullTank is not null)
            entry.IsFullTank = body.IsFullTank.Value;
        if (body.StationName is not null)
            entry.StationName = string.IsNullOrWhiteSpace(body.StationName) ? null : body.StationName.Trim();
        if (body.Notes is not null)
            entry.Notes = string.IsNullOrWhiteSpace(body.Notes) ? null : body.Notes.Trim();

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new FuelingEntryDto(
            entry.Id,
            entry.CarId,
            entry.PerformedAt,
            entry.KmAtFueling,
            entry.Liters,
            entry.TotalPrice,
            entry.FuelType,
            entry.IsFullTank,
            entry.StationName,
            entry.Notes);
    }
}
