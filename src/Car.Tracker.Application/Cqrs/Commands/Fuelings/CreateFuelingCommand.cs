using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;

namespace Car.Tracker.Application.Cqrs.Commands.Fuelings;

public sealed record CreateFuelingCommand(Guid CarId, CreateFuelingEntryRequest Body) : Request<CreateFuelingCommand, FuelingEntryDto?>;
