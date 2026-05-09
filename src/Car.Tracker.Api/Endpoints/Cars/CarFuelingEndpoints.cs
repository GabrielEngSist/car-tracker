using Car.Tracker.Api;
using Car.Tracker.Application.Cqrs.Commands.Fuelings;
using Car.Tracker.Application.Cqrs.Queries.Fuelings;
using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;

namespace Car.Tracker.Api.Endpoints.Cars;

internal static class CarFuelingEndpoints
{
    internal static RouteGroupBuilder MapCarFuelingEndpoints(this RouteGroupBuilder cars)
    {
        cars.MapGet("/{carId:guid}/fuelings", async (Guid carId, IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<GetCarFuelingsQuery, IReadOnlyList<FuelingEntryDto>?>(new GetCarFuelingsQuery(carId), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            var items = r.Value;
            return items is null ? Results.NotFound() : Results.Ok(items);
        });

        cars.MapPost("/{carId:guid}/fuelings", async (Guid carId, CreateFuelingEntryRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<CreateFuelingCommand, FuelingEntryDto?>(new CreateFuelingCommand(carId, request), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            var created = r.Value;
            return created is null ? Results.NotFound() : Results.Created($"/api/cars/{carId}/fuelings/{created.Id}", created);
        });

        cars.MapPatch("/{carId:guid}/fuelings/{fuelingId:guid}", async (Guid carId, Guid fuelingId, UpdateFuelingEntryRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<UpdateFuelingCommand, FuelingEntryDto?>(new UpdateFuelingCommand(carId, fuelingId, request), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            var updated = r.Value;
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        cars.MapDelete("/{carId:guid}/fuelings/{fuelingId:guid}", async (Guid carId, Guid fuelingId, IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<DeleteFuelingCommand, bool>(new DeleteFuelingCommand(carId, fuelingId), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            return r.Value! ? Results.NoContent() : Results.NotFound();
        });

        return cars;
    }
}
