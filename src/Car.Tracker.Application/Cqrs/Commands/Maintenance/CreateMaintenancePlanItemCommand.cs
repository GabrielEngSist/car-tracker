using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;

namespace Car.Tracker.Application.Cqrs.Commands.Maintenance;

public sealed record CreateMaintenancePlanItemCommand(Guid CarId, CreateMaintenancePlanItemRequest Body) : Request<CreateMaintenancePlanItemCommand, MaintenancePlanItemDto?>;
