using Car.Tracker.Application.Mediator;

namespace Car.Tracker.Application.Cqrs.Commands.Cars;

public sealed record DeleteCarCommand(Guid CarId) : Request<DeleteCarCommand, bool>;
