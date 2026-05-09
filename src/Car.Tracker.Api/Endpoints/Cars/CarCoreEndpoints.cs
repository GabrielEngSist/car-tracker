using Car.Tracker.Api;
using Car.Tracker.Application.Cqrs.Commands.Cars;
using Car.Tracker.Application.Cqrs.Queries.Cars;
using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;

namespace Car.Tracker.Api.Endpoints.Cars;

internal static class CarCoreEndpoints
{
    internal static RouteGroupBuilder MapCarCoreEndpoints(this RouteGroupBuilder cars)
    {
        cars.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<GetCarsQuery, IReadOnlyList<CarDto>>(new GetCarsQuery(), ct);
            return r.IsFailure ? MediatorHttp.ValidationProblem(r) : Results.Ok(r.Value);
        });

        cars.MapPost("/", async (CreateCarRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<CreateCarCommand, CreateCarOutcome>(new CreateCarCommand(request), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            var outcome = r.Value!;
            return outcome.Status switch
            {
                CreateCarStatus.Created => Results.Created($"/api/cars/{outcome.Car!.Id}", outcome.Car),
                CreateCarStatus.BadRequest => Results.BadRequest(outcome.Message),
                CreateCarStatus.BadGateway => Results.Problem(detail: outcome.Message, statusCode: StatusCodes.Status502BadGateway),
                _ => Results.Problem(statusCode: 500),
            };
        });

        cars.MapGet("/{carId:guid}", async (Guid carId, IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<GetCarByIdQuery, CarDto?>(new GetCarByIdQuery(carId), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            var car = r.Value;
            return car is null ? Results.NotFound() : Results.Ok(car);
        });

        cars.MapGet("/{carId:guid}/registry", async (Guid carId, IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<GetCarRegistryQuery, CarRegistryDto?>(new GetCarRegistryQuery(carId), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            var registry = r.Value;
            return registry is null ? Results.NotFound() : Results.Ok(registry);
        });

        cars.MapPatch("/{carId:guid}", async (Guid carId, UpdateCarRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<UpdateCarCommand, CarDto?>(new UpdateCarCommand(carId, request), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            var updated = r.Value;
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        cars.MapDelete("/{carId:guid}", async (Guid carId, IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<DeleteCarCommand, bool>(new DeleteCarCommand(carId), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            return r.Value! ? Results.NoContent() : Results.NotFound();
        });

        return cars;
    }
}
