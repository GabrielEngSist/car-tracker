using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Commands.Maintenance;

public sealed class UpdateMaintenancePlanItemCommandHandler(ITrackerPersistence db) : IRequestHandler<UpdateMaintenancePlanItemCommand, MaintenancePlanItemDto?>
{
    public async Task<HandlerResult<MaintenancePlanItemDto?>> Handle(UpdateMaintenancePlanItemCommand request, CancellationToken cancellationToken)
    {
        var item = await db.GetMaintenancePlanItemByCarAndIdTrackedAsync(request.CarId, request.PlanId, cancellationToken).ConfigureAwait(false);
        if (item is null) return RequestOutcome.Ok<MaintenancePlanItemDto?>(null);

        var body = request.Body;
        if (body.Title is not null)
        {
            if (string.IsNullOrWhiteSpace(body.Title))
                return RequestOutcome.Fail<MaintenancePlanItemDto?>("TITLE_EMPTY", "Title cannot be empty.", nameof(body.Title));
            item.Title = body.Title.Trim();
        }
        if (body.DueKmInterval is not null)
        {
            if (body.DueKmInterval <= 0)
                return RequestOutcome.Fail<MaintenancePlanItemDto?>("DUE_KM_INVALID", "DueKmInterval must be > 0.", nameof(body.DueKmInterval));
            item.DueKmInterval = body.DueKmInterval;
        }
        if (body.DueTimeIntervalDays is not null)
        {
            if (body.DueTimeIntervalDays <= 0)
                return RequestOutcome.Fail<MaintenancePlanItemDto?>("DUE_DAYS_INVALID", "DueTimeIntervalDays must be > 0.", nameof(body.DueTimeIntervalDays));
            item.DueTimeIntervalDays = body.DueTimeIntervalDays;
        }
        if (body.Active is not null) item.Active = body.Active.Value;

        var km = item.DueKmInterval;
        var days = item.DueTimeIntervalDays;
        if (km is null && days is null)
            return RequestOutcome.Fail<MaintenancePlanItemDto?>("INTERVAL_REQUIRED", "At least one interval is required (km or days).", nameof(body.DueKmInterval));

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return RequestOutcome.Ok<MaintenancePlanItemDto?>(
            new MaintenancePlanItemDto(item.Id, item.CarId, item.Title, item.DueKmInterval, item.DueTimeIntervalDays, item.Active));
    }
}
