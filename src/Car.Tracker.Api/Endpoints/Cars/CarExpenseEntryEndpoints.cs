using Car.Tracker.Api;
using Car.Tracker.Application.Cqrs.Commands.ExpenseEntries;
using Car.Tracker.Application.Cqrs.Queries.ExpenseEntries;
using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;

namespace Car.Tracker.Api.Endpoints.Cars;

internal static class CarExpenseEntryEndpoints
{
    internal static RouteGroupBuilder MapCarExpenseEntryEndpoints(this RouteGroupBuilder cars)
    {
        cars.MapGet("/{carId:guid}/entries", async (Guid carId, IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<GetCarExpenseEntriesQuery, IReadOnlyList<ExpenseEntryDto>?>(new GetCarExpenseEntriesQuery(carId), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            var items = r.Value;
            return items is null ? Results.NotFound() : Results.Ok(items);
        });

        cars.MapPost("/{carId:guid}/entries", async (Guid carId, CreateExpenseEntryRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<CreateExpenseEntryCommand, ExpenseEntryDto?>(new CreateExpenseEntryCommand(carId, request), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            var created = r.Value;
            return created is null ? Results.NotFound() : Results.Created($"/api/cars/{carId}/entries/{created.Id}", created);
        });

        cars.MapPatch("/{carId:guid}/entries/{entryId:guid}", async (Guid carId, Guid entryId, UpdateExpenseEntryRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<UpdateExpenseEntryCommand, ExpenseEntryDto?>(new UpdateExpenseEntryCommand(carId, entryId, request), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            var updated = r.Value;
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        cars.MapDelete("/{carId:guid}/entries/{entryId:guid}", async (Guid carId, Guid entryId, IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<DeleteExpenseEntryCommand, bool>(new DeleteExpenseEntryCommand(carId, entryId), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            return r.Value! ? Results.NoContent() : Results.NotFound();
        });

        return cars;
    }
}
