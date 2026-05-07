using Car.Tracker.Application.Common;
using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Commands.Maintenance;

public sealed class UpdateMaintenancePlanItemCommandHandler(ITrackerPersistence db) : IRequestHandler<UpdateMaintenancePlanItemCommand, MaintenancePlanItemDto?>
{
    public async Task<MaintenancePlanItemDto?> Handle(UpdateMaintenancePlanItemCommand request, CancellationToken cancellationToken)
    {
        var item = await db.GetMaintenancePlanItemByCarAndIdTrackedAsync(request.CarId, request.PlanId, cancellationToken).ConfigureAwait(false);
        if (item is null) return null;

        var body = request.Body;
        if (body.Title is not null)
        {
            if (string.IsNullOrWhiteSpace(body.Title)) throw new ValidationException("Title cannot be empty.");
            item.Title = body.Title.Trim();
        }
        if (body.DueKmInterval is not null)
        {
            if (body.DueKmInterval <= 0) throw new ValidationException("DueKmInterval must be > 0.");
            item.DueKmInterval = body.DueKmInterval;
        }
        if (body.DueTimeIntervalDays is not null)
        {
            if (body.DueTimeIntervalDays <= 0) throw new ValidationException("DueTimeIntervalDays must be > 0.");
            item.DueTimeIntervalDays = body.DueTimeIntervalDays;
        }
        if (body.Active is not null) item.Active = body.Active.Value;

        var km = item.DueKmInterval;
        var days = item.DueTimeIntervalDays;
        if (km is null && days is null)
            throw new ValidationException("At least one interval is required (km or days).");

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new MaintenancePlanItemDto(item.Id, item.CarId, item.Title, item.DueKmInterval, item.DueTimeIntervalDays, item.Active);
    }
}
