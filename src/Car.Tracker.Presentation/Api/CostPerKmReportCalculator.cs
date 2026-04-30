using Car.Tracker.Presentation.Domain;

namespace Car.Tracker.Presentation.Api;

public static class CostPerKmReportCalculator
{
    public static CostPerKmReportDto ComputeByPeriod(
        Guid carId,
        CarEntity car,
        IReadOnlyList<ExpenseEntry> expenseAll,
        IReadOnlyList<FuelingEntry> fuelAll,
        PeriodAggregator aggregator,
        DateOnly referenceDateUtc,
        DistanceReferenceMultiplier distanceMultiplier)
        => Compute(
            carId,
            car,
            expenseAll,
            fuelAll,
            modePeriod: true,
            periodAggregator: aggregator,
            distanceModeLifetime: false,
            distanceMultiplier: distanceMultiplier,
            referenceDateUtc);

    public static CostPerKmReportDto ComputeByDistanceLifetime(
        Guid carId,
        CarEntity car,
        IReadOnlyList<ExpenseEntry> expenseAll,
        IReadOnlyList<FuelingEntry> fuelAll,
        DistanceReferenceMultiplier distanceMultiplier,
        DateOnly referenceDateUtc)
        => Compute(
            carId,
            car,
            expenseAll,
            fuelAll,
            modePeriod: false,
            periodAggregator: PeriodAggregator.Total,
            distanceModeLifetime: true,
            distanceMultiplier: distanceMultiplier,
            referenceDateUtc);

    private static CostPerKmReportDto Compute(
        Guid carId,
        CarEntity car,
        IReadOnlyList<ExpenseEntry> expenseAll,
        IReadOnlyList<FuelingEntry> fuelAll,
        bool modePeriod,
        PeriodAggregator periodAggregator,
        bool distanceModeLifetime,
        DistanceReferenceMultiplier distanceMultiplier,
        DateOnly referenceDateUtc)
    {
        var carCreatedDate = DateOnly.FromDateTime(car.CreatedAt.UtcDateTime);

        var windowEnd = referenceDateUtc;
        DateOnly windowStart;

        if (distanceModeLifetime)
        {
            var minDates = expenseAll.Select(e => e.PerformedAt)
                .Concat(fuelAll.Select(f => f.PerformedAt))
                .DefaultIfEmpty(referenceDateUtc);
            windowStart = minDates.Min();
            if (windowStart < carCreatedDate)
                windowStart = carCreatedDate;
        }
        else
        {
            windowStart = periodAggregator switch
            {
                PeriodAggregator.Total => DetermineHistoryStart(carCreatedDate, expenseAll, fuelAll),
                PeriodAggregator.OneDay => referenceDateUtc.AddDays(-1),
                PeriodAggregator.OneMonth => referenceDateUtc.AddDays(-30),
                PeriodAggregator.SixMonths => referenceDateUtc.AddDays(-182),
                PeriodAggregator.OneYear => referenceDateUtc.AddDays(-365),
                _ => DetermineHistoryStart(carCreatedDate, expenseAll, fuelAll),
            };

            if (windowStart > windowEnd)
                windowStart = windowEnd;

            var historyStartFloor = DetermineHistoryStart(carCreatedDate, expenseAll, fuelAll);
            if (periodAggregator != PeriodAggregator.Total && windowStart < historyStartFloor)
                windowStart = historyStartFloor;
        }

        var expensesInWindow = expenseAll
            .Where(e => e.PerformedAt <= windowEnd && e.PerformedAt >= windowStart)
            .ToList();

        var fuelInWindow = fuelAll
            .Where(f => f.PerformedAt <= windowEnd && f.PerformedAt >= windowStart)
            .ToList();

        var expenseTotal = expensesInWindow.Sum(x => x.Price);
        var fuelTotal = fuelInWindow.Sum(x => x.TotalPrice);
        var grand = expenseTotal + fuelTotal;

        var kmPoints = new List<int>(
            expensesInWindow.Select(e => e.KmAtService).Concat(fuelInWindow.Select(f => f.KmAtFueling)));

        int? minKm = kmPoints.Count > 0 ? kmPoints.Min() : null;
        int? maxKm = kmPoints.Count > 0 ? kmPoints.Max() : null;
        int? kmDelta = minKm.HasValue && maxKm.HasValue ? maxKm.Value - minKm.Value : null;

        decimal? costPerKm =
            kmDelta is > 0 ? decimal.Round(grand / kmDelta.Value, 8, MidpointRounding.AwayFromZero) : null;

        int? refKm = distanceMultiplier switch
        {
            DistanceReferenceMultiplier.OneKm => 1,
            DistanceReferenceMultiplier.TenKm => 10,
            DistanceReferenceMultiplier.OneHundredKm => 100,
            DistanceReferenceMultiplier.OneThousandKm => 1000,
            DistanceReferenceMultiplier.Total => null,
            _ => null,
        };

        decimal? estAtRef =
            costPerKm is not null && refKm is > 0
                ? decimal.Round(costPerKm.Value * refKm.Value, 8, MidpointRounding.AwayFromZero)
                : null;

        var modeKey = distanceModeLifetime ? "distance" : "period";
        string periodVariant = distanceModeLifetime ? "lifetime" : periodAggregator.ToString("G").ToLowerInvariant();
        string distVariant = distanceMultiplier.ToString("G").ToLowerInvariant();

        var label = $"{(distanceModeLifetime ? "Vida útil registada" : LabelPeriod(periodAggregator))} · {LabelDistance(distanceMultiplier)}";

        return new CostPerKmReportDto(
            CarId: carId,
            Mode: modeKey,
            PeriodVariant: periodVariant,
            DistanceReferenceVariant: distVariant,
            AggregatorLabel: label,
            WindowStartInclusive: windowStart,
            WindowEndInclusive: windowEnd,
            MaintenanceExpenseTotal: expenseTotal,
            FuelExpenseTotal: fuelTotal,
            GrandTotalExpense: grand,
            ExpenseEntryCountIncluded: expensesInWindow.Count,
            FuelingEntryCountIncluded: fuelInWindow.Count,
            MinKmObservedInWindow: minKm,
            MaxKmObservedInWindow: maxKm,
            KmDelta: kmDelta,
            CostPerKm: costPerKm,
            DistanceReferenceKm: refKm,
            EstimatedCostAtDistanceReference: distanceMultiplier == DistanceReferenceMultiplier.Total ? null : estAtRef);
    }

    private static DateOnly DetermineHistoryStart(
        DateOnly carCreatedDate,
        IReadOnlyList<ExpenseEntry> expenseAll,
        IReadOnlyList<FuelingEntry> fuelAll)
    {
        var dates = expenseAll.Select(e => e.PerformedAt)
            .Concat(fuelAll.Select(f => f.PerformedAt))
            .ToList();

        if (dates.Count == 0)
            return carCreatedDate;

        var earliest = dates.Min();
        return earliest < carCreatedDate ? carCreatedDate : earliest;
    }

    private static string LabelPeriod(PeriodAggregator p) =>
        p switch
        {
            PeriodAggregator.Total => "Período: total registado",
            PeriodAggregator.OneDay => "Últimos 1 dia",
            PeriodAggregator.OneMonth => "Últimos 30 dias",
            PeriodAggregator.SixMonths => "Últimos ~6 meses",
            PeriodAggregator.OneYear => "Últimos ~1 ano",
            _ => "",
        };

    private static string LabelDistance(DistanceReferenceMultiplier r) =>
        r switch
        {
            DistanceReferenceMultiplier.Total => "Referência km: custo absoluto da janela",
            DistanceReferenceMultiplier.OneKm => "Referência: 1 km",
            DistanceReferenceMultiplier.TenKm => "Referência: 10 km",
            DistanceReferenceMultiplier.OneHundredKm => "Referência: 100 km",
            DistanceReferenceMultiplier.OneThousandKm => "Referência: 1000 km",
            _ => "",
        };
}
