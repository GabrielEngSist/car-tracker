using Car.Tracker.Application.Mediator;
using Car.Tracker.Domain.Ports.Persistence;
using Car.Tracker.Domain.Reports;

namespace Car.Tracker.Application.Cqrs.Queries.Reports;

public sealed class GetFuelFullTankReportQueryHandler(ITrackerPersistence db) : IRequestHandler<GetFuelFullTankReportQuery, FuelFullTankEfficiencyReportDto?>
{
    public async Task<HandlerResult<FuelFullTankEfficiencyReportDto?>> Handle(GetFuelFullTankReportQuery request, CancellationToken cancellationToken)
    {
        if (!CostPerKmReportQuery.ParseBasis(request.Basis, out var lifetimeMode))
            return RequestOutcome.Fail<FuelFullTankEfficiencyReportDto?>("BASIS_INVALID", "basis must be 'period' or 'lifetime'.", nameof(request.Basis));

        PeriodAggregator periodAgg = PeriodAggregator.Total;
        if (!lifetimeMode)
        {
            if (!CostPerKmReportQuery.TryParsePeriod(request.Period, out periodAgg))
                return RequestOutcome.Fail<FuelFullTankEfficiencyReportDto?>("PERIOD_INVALID", "period invalid. Use total, 1d, 1m, 6m, 1y.", nameof(request.Period));
        }

        var car = await db.GetCarByIdReadOnlyAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        if (car is null) return RequestOutcome.Ok<FuelFullTankEfficiencyReportDto?>(null);

        var fuels = await db.GetFuelingsForReportsAsync(request.CarId, cancellationToken).ConfigureAwait(false);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var report = lifetimeMode
            ? FuelFullTankEfficiencyCalculator.Compute(request.CarId, car, fuels, lifetimeMode: true, PeriodAggregator.Total, today)
            : FuelFullTankEfficiencyCalculator.Compute(request.CarId, car, fuels, lifetimeMode: false, periodAgg, today);

        return RequestOutcome.Ok<FuelFullTankEfficiencyReportDto?>(report);
    }
}
