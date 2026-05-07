using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;

namespace Car.Tracker.Application.Cqrs.Queries.Maintenance;

public sealed record GetMaintenanceStatusQuery(Guid CarId) : Request<GetMaintenanceStatusQuery, IReadOnlyList<MaintenanceStatusDto>?>;
