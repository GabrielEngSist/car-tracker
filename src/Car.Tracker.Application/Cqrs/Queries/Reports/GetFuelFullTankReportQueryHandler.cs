using Car.Tracker.Application.Common;
using Car.Tracker.Application.Mediator;
using Car.Tracker.Domain.Ports.Persistence;
using Car.Tracker.Domain.Reports;

namespace Car.Tracker.Application.Cqrs.Queries.Reports;

public sealed class GetFuelFullTankReportQueryHandler(ITrackerPersistence db) : IRequestHandler<GetFuelFullTankReportQuery, FuelFullTankEfficiencyReportDto?>
{
    public async Task<FuelFullTankEfficiencyReportDto?> Handle(GetFuelFullTankReportQuery request, CancellationToken cancellationToken)
    {
        if (!CostPerKmReportQuery.ParseBasis(request.Basis, out var lifetimeMode))
            throw new ValidationException("basis must be 'period' or 'lifetime'.");

        PeriodAggregator periodAgg = PeriodAggregator.Total;
        if (!lifetimeMode)
        {
            if (!CostPerKmReportQuery.TryParsePeriod(request.Period, out periodAgg))
                throw new ValidationException("period invalid. Use total, 1d, 1m, 6m, 1y.");
        }

        var car = await db.GetCarByIdReadOnlyAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        if (car is null) return null;

        var fuels = await db.GetFuelingsForReportsAsync(request.CarId, cancellationToken).ConfigureAwait(false);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return lifetimeMode
            ? FuelFullTankEfficiencyCalculator.Compute(request.CarId, car, fuels, lifetimeMode: true, PeriodAggregator.Total, today)
            : FuelFullTankEfficiencyCalculator.Compute(request.CarId, car, fuels, lifetimeMode: false, periodAgg, today);
    }
}
