using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;

namespace Car.Tracker.Application.Cqrs.Commands.Maintenance;

public sealed record UpdateMaintenancePlanItemCommand(Guid CarId, Guid PlanId, UpdateMaintenancePlanItemRequest Body) : Request<UpdateMaintenancePlanItemCommand, MaintenancePlanItemDto?>;
