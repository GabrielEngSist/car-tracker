using Car.Tracker.Api;
using Car.Tracker.Application.Cqrs.Queries.Reports;
using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Reports;

namespace Car.Tracker.Api.Endpoints.Cars;

internal static class CarReportEndpoints
{
    internal static RouteGroupBuilder MapCarReportEndpoints(this RouteGroupBuilder cars)
    {
        cars.MapGet("/{carId:guid}/reports/fuel-full-tank", async (
            Guid carId,
            string? basis,
            string? period,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<GetFuelFullTankReportQuery, FuelFullTankEfficiencyReportDto?>(new GetFuelFullTankReportQuery(carId, basis, period), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            var report = r.Value;
            return report is null ? Results.NotFound() : Results.Ok(report);
        });

        cars.MapGet("/{carId:guid}/reports/cost-per-km", async (
            Guid carId,
            string? basis,
            string? period,
            string? distanceRef,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var r = await mediator.SendAsync<GetCostPerKmReportQuery, CostPerKmReportDto?>(new GetCostPerKmReportQuery(carId, basis, period, distanceRef), ct);
            if (r.IsFailure)
                return MediatorHttp.ValidationProblem(r);
            var report = r.Value;
            return report is null ? Results.NotFound() : Results.Ok(report);
        });

        return cars;
    }
}
