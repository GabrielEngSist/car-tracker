using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Entities;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Commands.Maintenance;

public sealed class CreateMaintenancePlanItemCommandHandler(ITrackerPersistence db) : IRequestHandler<CreateMaintenancePlanItemCommand, MaintenancePlanItemDto?>
{
    public async Task<HandlerResult<MaintenancePlanItemDto?>> Handle(CreateMaintenancePlanItemCommand request, CancellationToken cancellationToken)
    {
        var exists = await db.CarExistsAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        if (!exists) return RequestOutcome.Ok<MaintenancePlanItemDto?>(null);

        var body = request.Body;
        if (string.IsNullOrWhiteSpace(body.Title))
            return RequestOutcome.Fail<MaintenancePlanItemDto?>("TITLE_REQUIRED", "Title is required.", nameof(body.Title));
        if (body.DueKmInterval is null && body.DueTimeIntervalDays is null)
            return RequestOutcome.Fail<MaintenancePlanItemDto?>("INTERVAL_REQUIRED", "At least one interval is required (km or days).", nameof(body.DueKmInterval));
        if (body.DueKmInterval is not null && body.DueKmInterval <= 0)
            return RequestOutcome.Fail<MaintenancePlanItemDto?>("DUE_KM_INVALID", "DueKmInterval must be > 0.", nameof(body.DueKmInterval));
        if (body.DueTimeIntervalDays is not null && body.DueTimeIntervalDays <= 0)
            return RequestOutcome.Fail<MaintenancePlanItemDto?>("DUE_DAYS_INVALID", "DueTimeIntervalDays must be > 0.", nameof(body.DueTimeIntervalDays));

        var item = new MaintenancePlanItem
        {
            CarId = request.CarId,
            Title = body.Title.Trim(),
            DueKmInterval = body.DueKmInterval,
            DueTimeIntervalDays = body.DueTimeIntervalDays,
            Active = body.Active,
        };

        db.AddMaintenancePlanItem(item);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return RequestOutcome.Ok<MaintenancePlanItemDto?>(
            new MaintenancePlanItemDto(item.Id, item.CarId, item.Title, item.DueKmInterval, item.DueTimeIntervalDays, item.Active));
    }
}
