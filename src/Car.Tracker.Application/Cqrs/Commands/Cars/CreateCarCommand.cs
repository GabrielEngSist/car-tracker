using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;

namespace Car.Tracker.Application.Cqrs.Commands.Cars;

public sealed record CreateCarCommand(CreateCarRequest Request) : IRequest<CreateCarOutcome>;
