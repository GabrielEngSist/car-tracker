using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Commands.Fuelings;

public sealed class UpdateFuelingCommandHandler(ITrackerPersistence db) : IRequestHandler<UpdateFuelingCommand, FuelingEntryDto?>
{
    public async Task<HandlerResult<FuelingEntryDto?>> Handle(UpdateFuelingCommand request, CancellationToken cancellationToken)
    {
        var entry = await db.GetFuelingByCarAndIdTrackedAsync(request.CarId, request.FuelingId, cancellationToken).ConfigureAwait(false);
        if (entry is null) return RequestOutcome.Ok<FuelingEntryDto?>(null);

        var body = request.Body;
        if (body.PerformedAt is not null) entry.PerformedAt = body.PerformedAt.Value;
        if (body.KmAtFueling is not null)
        {
            if (body.KmAtFueling.Value < 0)
                return RequestOutcome.Fail<FuelingEntryDto?>("INVALID_KM", "KmAtFueling is invalid.", nameof(body.KmAtFueling));
            entry.KmAtFueling = body.KmAtFueling.Value;
        }
        if (body.Liters is not null)
        {
            if (body.Liters.Value <= 0)
                return RequestOutcome.Fail<FuelingEntryDto?>("INVALID_LITERS", "Liters must be > 0.", nameof(body.Liters));
            entry.Liters = body.Liters.Value;
        }
        if (body.TotalPrice is not null)
        {
            if (body.TotalPrice.Value < 0)
                return RequestOutcome.Fail<FuelingEntryDto?>("INVALID_TOTAL_PRICE", "TotalPrice is invalid.", nameof(body.TotalPrice));
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
