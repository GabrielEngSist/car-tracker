using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Queries.Maintenance;

public sealed class GetMaintenancePlansQueryHandler(ITrackerPersistence db) : IRequestHandler<GetMaintenancePlansQuery, IReadOnlyList<MaintenancePlanItemDto>?>
{
    public async Task<HandlerResult<IReadOnlyList<MaintenancePlanItemDto>?>> Handle(GetMaintenancePlansQuery request, CancellationToken cancellationToken)
    {
        var exists = await db.CarExistsAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        if (!exists) return RequestOutcome.Ok<IReadOnlyList<MaintenancePlanItemDto>?>(null);

        var list = await db.GetMaintenancePlanItemsForPlansEndpointAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        var dtos = list
            .Select(x => new MaintenancePlanItemDto(x.Id, x.CarId, x.Title, x.DueKmInterval, x.DueTimeIntervalDays, x.Active))
            .ToList();
        return RequestOutcome.Ok<IReadOnlyList<MaintenancePlanItemDto>?>(dtos);
    }
}
