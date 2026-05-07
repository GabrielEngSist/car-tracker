using Car.Tracker.Application.Mediator;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Commands.Maintenance;

public sealed class DeleteMaintenancePlanItemCommandHandler(ITrackerPersistence db) : IRequestHandler<DeleteMaintenancePlanItemCommand, bool>
{
    public async Task<bool> Handle(DeleteMaintenancePlanItemCommand request, CancellationToken cancellationToken)
    {
        var item = await db.GetMaintenancePlanItemByCarAndIdTrackedAsync(request.CarId, request.PlanId, cancellationToken).ConfigureAwait(false);
        if (item is null) return false;
        db.RemoveMaintenancePlanItem(item);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
