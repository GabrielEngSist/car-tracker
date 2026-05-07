using Car.Tracker.Application.Mediator;

namespace Car.Tracker.Application.Cqrs.Commands.Maintenance;

public sealed record DeleteMaintenancePlanItemCommand(Guid CarId, Guid PlanId) : Request<DeleteMaintenancePlanItemCommand, bool>;
