using Car.Tracker.Application.Common;
using Car.Tracker.Application.Mediator;
using Car.Tracker.Domain.Ports.Persistence;
using Car.Tracker.Domain.Reports;

namespace Car.Tracker.Application.Cqrs.Queries.Reports;

public sealed class GetCostPerKmReportQueryHandler(ITrackerPersistence db) : IRequestHandler<GetCostPerKmReportQuery, CostPerKmReportDto?>
{
    public async Task<CostPerKmReportDto?> Handle(GetCostPerKmReportQuery request, CancellationToken cancellationToken)
    {
        if (!CostPerKmReportQuery.ParseBasis(request.Basis, out var lifetimeMode))
            throw new ValidationException("basis must be 'period' or 'lifetime'.");

        if (!CostPerKmReportQuery.TryParseDistanceRef(request.DistanceRef, out var dMult))
            throw new ValidationException("distanceRef invalid. Use total, km1, km10, km100, km1000 (or 1,10,100,1000).");

        PeriodAggregator periodAgg = PeriodAggregator.Total;
        if (!lifetimeMode)
        {
            if (!CostPerKmReportQuery.TryParsePeriod(request.Period, out periodAgg))
                throw new ValidationException("period invalid. Use total, 1d, 1m, 6m, 1y.");
        }

        var car = await db.GetCarByIdReadOnlyAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        if (car is null) return null;

        var expenses = await db.GetExpenseEntriesForReportsAsync(request.CarId, cancellationToken).ConfigureAwait(false);

        var fuels = await db.GetFuelingsForReportsAsync(request.CarId, cancellationToken).ConfigureAwait(false);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return lifetimeMode
            ? CostPerKmReportCalculator.ComputeByDistanceLifetime(request.CarId, car, expenses, fuels, dMult, today)
            : CostPerKmReportCalculator.ComputeByPeriod(request.CarId, car, expenses, fuels, periodAgg, today, dMult);
    }
}
