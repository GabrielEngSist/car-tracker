using Car.Tracker.Application.Mediator;
using Car.Tracker.Domain.Ports.Persistence;
using Car.Tracker.Domain.Reports;

namespace Car.Tracker.Application.Cqrs.Queries.Reports;

public sealed class GetCostPerKmReportQueryHandler(ITrackerPersistence db) : IRequestHandler<GetCostPerKmReportQuery, CostPerKmReportDto?>
{
    public async Task<HandlerResult<CostPerKmReportDto?>> Handle(GetCostPerKmReportQuery request, CancellationToken cancellationToken)
    {
        if (!CostPerKmReportQuery.ParseBasis(request.Basis, out var lifetimeMode))
            return RequestOutcome.Fail<CostPerKmReportDto?>("BASIS_INVALID", "basis must be 'period' or 'lifetime'.", nameof(request.Basis));

        if (!CostPerKmReportQuery.TryParseDistanceRef(request.DistanceRef, out var dMult))
            return RequestOutcome.Fail<CostPerKmReportDto?>("DISTANCE_REF_INVALID", "distanceRef invalid. Use total, km1, km10, km100, km1000 (or 1,10,100,1000).", nameof(request.DistanceRef));

        PeriodAggregator periodAgg = PeriodAggregator.Total;
        if (!lifetimeMode)
        {
            if (!CostPerKmReportQuery.TryParsePeriod(request.Period, out periodAgg))
                return RequestOutcome.Fail<CostPerKmReportDto?>("PERIOD_INVALID", "period invalid. Use total, 1d, 1m, 6m, 1y.", nameof(request.Period));
        }

        var car = await db.GetCarByIdReadOnlyAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        if (car is null) return RequestOutcome.Ok<CostPerKmReportDto?>(null);

        var expenses = await db.GetExpenseEntriesForReportsAsync(request.CarId, cancellationToken).ConfigureAwait(false);

        var fuels = await db.GetFuelingsForReportsAsync(request.CarId, cancellationToken).ConfigureAwait(false);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var report = lifetimeMode
            ? CostPerKmReportCalculator.ComputeByDistanceLifetime(request.CarId, car, expenses, fuels, dMult, today)
            : CostPerKmReportCalculator.ComputeByPeriod(request.CarId, car, expenses, fuels, periodAgg, today, dMult);

        return RequestOutcome.Ok<CostPerKmReportDto?>(report);
    }
}
