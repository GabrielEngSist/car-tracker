using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;

namespace Car.Tracker.Application.Cqrs.Queries.Fuelings;

public sealed record GetCarFuelingsQuery(Guid CarId) : Request<GetCarFuelingsQuery, IReadOnlyList<FuelingEntryDto>?>;
