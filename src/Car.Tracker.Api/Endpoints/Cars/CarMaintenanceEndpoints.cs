using Car.Tracker.Api;
using Car.Tracker.Application.Cqrs.Commands.Maintenance;
using Car.Tracker.Application.Cqrs.Queries.Maintenance;
using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;

namespace Car.Tracker.Api.Endpoints.Cars;

internal static class CarMaintenanceEndpoints
{
    internal static RouteGroupBuilder MapCarMaintenanceEndpoints(this RouteGroupBuilder cars)
    {
        cars.MapGet("/{carId:guid}/maintenance-plans", async (Guid carId, IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<GetMaintenancePlansQuery, IReadOnlyList<MaintenancePlanItemDto>?>(new GetMaintenancePlansQuery(carId), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            var items = r.Value;
            return items is null ? Results.NotFound() : Results.Ok(items);
        });

        cars.MapPost("/{carId:guid}/maintenance-plans", async (Guid carId, CreateMaintenancePlanItemRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<CreateMaintenancePlanItemCommand, MaintenancePlanItemDto?>(new CreateMaintenancePlanItemCommand(carId, request), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            var created = r.Value;
            return created is null ? Results.NotFound() : Results.Created($"/api/cars/{carId}/maintenance-plans/{created.Id}", created);
        });

        cars.MapPatch("/{carId:guid}/maintenance-plans/{planId:guid}", async (Guid carId, Guid planId, UpdateMaintenancePlanItemRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<UpdateMaintenancePlanItemCommand, MaintenancePlanItemDto?>(new UpdateMaintenancePlanItemCommand(carId, planId, request), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            var updated = r.Value;
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        cars.MapDelete("/{carId:guid}/maintenance-plans/{planId:guid}", async (Guid carId, Guid planId, IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<DeleteMaintenancePlanItemCommand, bool>(new DeleteMaintenancePlanItemCommand(carId, planId), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            return r.Value! ? Results.NoContent() : Results.NotFound();
        });

        cars.MapGet("/{carId:guid}/maintenance-status", async (Guid carId, IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<GetMaintenanceStatusQuery, IReadOnlyList<MaintenanceStatusDto>?>(new GetMaintenanceStatusQuery(carId), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            var status = r.Value;
            return status is null ? Results.NotFound() : Results.Ok(status);
        });

        return cars;
    }
}
