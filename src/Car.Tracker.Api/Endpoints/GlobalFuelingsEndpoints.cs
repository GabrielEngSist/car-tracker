using Car.Tracker.Api;
using Car.Tracker.Application.Cqrs.Queries.Fuelings;
using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;

namespace Car.Tracker.Api.Endpoints;

internal static class GlobalFuelingsEndpoints
{
    internal static WebApplication MapGlobalFuelingsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/fuelings", async (IMediator mediator, CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<GetAllFuelingsQuery, IReadOnlyList<FuelingEntryDto>>(new GetAllFuelingsQuery(), ct);
            return r.IsFailure ? MediatorHttp.ValidationProblem(r) : Results.Ok(r.Value);
        });
        return app;
    }
}
