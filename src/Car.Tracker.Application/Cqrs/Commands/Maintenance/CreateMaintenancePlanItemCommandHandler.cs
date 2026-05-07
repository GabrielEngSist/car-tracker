using Car.Tracker.Application.Common;
using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Entities;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Commands.Maintenance;

public sealed class CreateMaintenancePlanItemCommandHandler(ITrackerPersistence db) : IRequestHandler<CreateMaintenancePlanItemCommand, MaintenancePlanItemDto?>
{
    public async Task<MaintenancePlanItemDto?> Handle(CreateMaintenancePlanItemCommand request, CancellationToken cancellationToken)
    {
        var exists = await db.CarExistsAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        if (!exists) return null;

        var body = request.Body;
        if (string.IsNullOrWhiteSpace(body.Title)) throw new ValidationException("Title is required.");
        if (body.DueKmInterval is null && body.DueTimeIntervalDays is null)
            throw new ValidationException("At least one interval is required (km or days).");
        if (body.DueKmInterval is not null && body.DueKmInterval <= 0) throw new ValidationException("DueKmInterval must be > 0.");
        if (body.DueTimeIntervalDays is not null && body.DueTimeIntervalDays <= 0) throw new ValidationException("DueTimeIntervalDays must be > 0.");

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

        return new MaintenancePlanItemDto(item.Id, item.CarId, item.Title, item.DueKmInterval, item.DueTimeIntervalDays, item.Active);
    }
}
