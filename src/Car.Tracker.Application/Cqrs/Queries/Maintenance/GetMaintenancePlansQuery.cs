using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;

namespace Car.Tracker.Application.Cqrs.Queries.Maintenance;

public sealed record GetMaintenancePlansQuery(Guid CarId) : Request<GetMaintenancePlansQuery, IReadOnlyList<MaintenancePlanItemDto>?>;
