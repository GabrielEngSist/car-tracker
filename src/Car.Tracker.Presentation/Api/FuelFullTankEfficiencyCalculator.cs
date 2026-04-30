using Car.Tracker.Presentation.Domain;

namespace Car.Tracker.Presentation.Api;

public static class FuelFullTankEfficiencyCalculator
{
    public static FuelFullTankEfficiencyReportDto Compute(
        Guid carId,
        CarEntity car,
        IReadOnlyList<FuelingEntry> fuelAll,
        bool lifetimeMode,
        PeriodAggregator periodAggregator,
        DateOnly referenceDateUtc)
    {
        var carCreatedDate = DateOnly.FromDateTime(car.CreatedAt.UtcDateTime);
        var windowEnd = referenceDateUtc;

        DateOnly windowStart;
        if (lifetimeMode)
        {
            windowStart = DetermineHistoryStartFuelOnly(carCreatedDate, fuelAll);
        }
        else
        {
            windowStart = periodAggregator switch
            {
                PeriodAggregator.Total => DetermineHistoryStartFuelOnly(carCreatedDate, fuelAll),
                PeriodAggregator.OneDay => referenceDateUtc.AddDays(-1),
                PeriodAggregator.OneMonth => referenceDateUtc.AddDays(-30),
                PeriodAggregator.SixMonths => referenceDateUtc.AddDays(-182),
                PeriodAggregator.OneYear => referenceDateUtc.AddDays(-365),
                _ => DetermineHistoryStartFuelOnly(carCreatedDate, fuelAll),
            };

            if (windowStart > windowEnd)
                windowStart = windowEnd;

            var historyStartFloor = DetermineHistoryStartFuelOnly(carCreatedDate, fuelAll);
            if (periodAggregator != PeriodAggregator.Total && windowStart < historyStartFloor)
                windowStart = historyStartFloor;
        }

        var sorted = fuelAll
            .OrderBy(f => f.PerformedAt)
            .ThenBy(f => f.Id)
            .ToList();

        var fullIndices = new List<int>();
        for (var i = 0; i < sorted.Count; i++)
        {
            if (sorted[i].IsFullTank)
                fullIndices.Add(i);
        }

        var fullTankInWindow = sorted.Count(f =>
            f.IsFullTank && f.PerformedAt >= windowStart && f.PerformedAt <= windowEnd);

        var perTypeLiters = Enum.GetValues<FuelType>().ToDictionary(t => t, _ => 0m);
        var perTypeAttributedKm = Enum.GetValues<FuelType>().ToDictionary(t => t, _ => 0m);
        var perTypeSpend = Enum.GetValues<FuelType>().ToDictionary(t => t, _ => 0m);

        var intervalDetails = new List<FuelFullTankIntervalDetailDto>();
        decimal sumDeltaKm = 0;
        decimal sumLitersSegments = 0;
        decimal sumPriceSegments = 0;

        for (var pair = 1; pair < fullIndices.Count; pair++)
        {
            var iStart = fullIndices[pair - 1];
            var iEnd = fullIndices[pair];
            var evStart = sorted[iStart];
            var evEnd = sorted[iEnd];

            if (evEnd.PerformedAt < windowStart || evEnd.PerformedAt > windowEnd)
                continue;

            var deltaKm = evEnd.KmAtFueling - evStart.KmAtFueling;
            if (deltaKm <= 0)
                continue;

            decimal litersSeg = 0;
            decimal priceSeg = 0;
            for (var j = iStart + 1; j <= iEnd; j++)
            {
                litersSeg += sorted[j].Liters;
                priceSeg += sorted[j].TotalPrice;
            }

            if (litersSeg <= 0)
                continue;

            var kmPerLiterSeg = decimal.Round((decimal)deltaKm / litersSeg, 6, MidpointRounding.AwayFromZero);
            var costKmSeg = decimal.Round(priceSeg / deltaKm, 6, MidpointRounding.AwayFromZero);

            intervalDetails.Add(new FuelFullTankIntervalDetailDto(
                SegmentIndex: intervalDetails.Count + 1,
                StartPerformedAt: evStart.PerformedAt,
                StartKmAtFueling: evStart.KmAtFueling,
                EndPerformedAt: evEnd.PerformedAt,
                EndKmAtFueling: evEnd.KmAtFueling,
                DeltaKm: deltaKm,
                TotalLitersInInterval: decimal.Round(litersSeg, 4, MidpointRounding.AwayFromZero),
                TotalFuelPriceInInterval: decimal.Round(priceSeg, 2, MidpointRounding.AwayFromZero),
                AverageKmPerLiter: kmPerLiterSeg,
                FuelCostPerKm: costKmSeg));

            sumDeltaKm += deltaKm;
            sumLitersSegments += litersSeg;
            sumPriceSegments += priceSeg;

            var shareDenom = litersSeg;
            for (var j = iStart + 1; j <= iEnd; j++)
            {
                var row = sorted[j];
                var share = shareDenom > 0 ? row.Liters / shareDenom : 0;
                var kmPart = deltaKm * share;

                perTypeLiters[row.FuelType] += row.Liters;
                perTypeAttributedKm[row.FuelType] += kmPart;
                perTypeSpend[row.FuelType] += row.TotalPrice;
            }
        }

        decimal? overallKmPerLiter =
            sumLitersSegments > 0 && sumDeltaKm > 0
                ? decimal.Round(sumDeltaKm / sumLitersSegments, 6, MidpointRounding.AwayFromZero)
                : null;

        decimal? overallLPer100Km =
            overallKmPerLiter is > 0
                ? decimal.Round(100m / overallKmPerLiter.Value, 6, MidpointRounding.AwayFromZero)
                : null;

        decimal? overallCostPerKm =
            sumDeltaKm > 0
                ? decimal.Round(sumPriceSegments / sumDeltaKm, 6, MidpointRounding.AwayFromZero)
                : null;

        var byType = new List<FuelTypeEfficiencyAggregateDto>();
        foreach (var ft in Enum.GetValues<FuelType>())
        {
            var L = perTypeLiters[ft];
            var kmA = perTypeAttributedKm[ft];
            var spend = perTypeSpend[ft];
            if (L <= 0 && spend <= 0 && kmA <= 0)
                continue;

            decimal? avgKmL = L > 0 ? decimal.Round(kmA / L, 6, MidpointRounding.AwayFromZero) : null;
            decimal? cKm = kmA > 0 ? decimal.Round(spend / kmA, 6, MidpointRounding.AwayFromZero) : null;

            byType.Add(new FuelTypeEfficiencyAggregateDto(
                FuelType: ft,
                TotalLiters: decimal.Round(L, 4, MidpointRounding.AwayFromZero),
                AttributedKm: decimal.Round(kmA, 4, MidpointRounding.AwayFromZero),
                AverageKmPerLiter: avgKmL,
                TotalFuelSpend: decimal.Round(spend, 2, MidpointRounding.AwayFromZero),
                FuelCostPerKm: cKm));
        }

        var modeKey = lifetimeMode ? "lifetime" : "period";
        var periodVariant = lifetimeMode ? "lifetime" : periodAggregator.ToString("G").ToLowerInvariant();
        var label = lifetimeMode
            ? "Autonomia (tanque cheio): vida útil registada"
            : $"Autonomia (tanque cheio): {LabelPeriod(periodAggregator)}";

        return new FuelFullTankEfficiencyReportDto(
            CarId: carId,
            Mode: modeKey,
            PeriodVariant: periodVariant,
            AggregatorLabel: label,
            WindowStartInclusive: windowStart,
            WindowEndInclusive: windowEnd,
            FullTankMarkersInWindow: fullTankInWindow,
            IntervalsUsedCount: intervalDetails.Count,
            OverallAverageKmPerLiter: overallKmPerLiter,
            OverallLitersPer100Km: overallLPer100Km,
            OverallFuelCostPerKm: overallCostPerKm,
            ByFuelType: byType,
            Intervals: intervalDetails);
    }

    private static DateOnly DetermineHistoryStartFuelOnly(DateOnly carCreatedDate, IReadOnlyList<FuelingEntry> fuelAll)
    {
        if (fuelAll.Count == 0)
            return carCreatedDate;

        var earliest = fuelAll.Min(f => f.PerformedAt);
        return earliest < carCreatedDate ? carCreatedDate : earliest;
    }

    private static string LabelPeriod(PeriodAggregator p) =>
        p switch
        {
            PeriodAggregator.Total => "total registado",
            PeriodAggregator.OneDay => "últimos 1 dia",
            PeriodAggregator.OneMonth => "últimos 30 dias",
            PeriodAggregator.SixMonths => "últimos ~6 meses",
            PeriodAggregator.OneYear => "últimos ~1 ano",
            _ => "",
        };
}
