using Car.Tracker.Application.Mediator;

namespace Car.Tracker.Application.Cqrs.Commands.Fuelings;

public sealed record DeleteFuelingCommand(Guid CarId, Guid FuelingId) : IRequest<bool>;
