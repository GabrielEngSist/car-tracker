using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;

namespace Car.Tracker.Application.Cqrs.Queries.Cars;

public sealed record GetCarByIdQuery(Guid CarId) : IRequest<CarDto?>;
