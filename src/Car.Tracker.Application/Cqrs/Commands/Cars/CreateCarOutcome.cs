using Car.Tracker.Contracts;

namespace Car.Tracker.Application.Cqrs.Commands.Cars;

public enum CreateCarStatus
{
    Created,
    BadRequest,
    BadGateway,
}

public sealed record CreateCarOutcome(CreateCarStatus Status, CarDto? Car, string? Message);
