using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;

namespace Car.Tracker.Application.Cqrs.Commands.Cars;

public sealed record UpdateCarCommand(Guid CarId, UpdateCarRequest Request) : Request<UpdateCarCommand, CarDto?>;
