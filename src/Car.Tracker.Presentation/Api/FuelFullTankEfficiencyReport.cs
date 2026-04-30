using Car.Tracker.Presentation.Domain;

namespace Car.Tracker.Presentation.Api;

public sealed record FuelTypeEfficiencyAggregateDto(
    FuelType FuelType,
    decimal TotalLiters,
    decimal AttributedKm,
    decimal? AverageKmPerLiter,
    decimal TotalFuelSpend,
    decimal? FuelCostPerKm);

/// <summary>
/// Um intervalo entre dois abastecimentos com tanque cheio consecutivos (fim do intervalo ancorado no período filtrado quando aplicável).
/// </summary>
public sealed record FuelFullTankIntervalDetailDto(
    int SegmentIndex,
    DateOnly StartPerformedAt,
    int StartKmAtFueling,
    DateOnly EndPerformedAt,
    int EndKmAtFueling,
    int DeltaKm,
    decimal TotalLitersInInterval,
    decimal TotalFuelPriceInInterval,
    decimal? AverageKmPerLiter,
    decimal? FuelCostPerKm);

public sealed record FuelFullTankEfficiencyReportDto(
    Guid CarId,
    string Mode,
    string PeriodVariant,
    string AggregatorLabel,
    DateOnly WindowStartInclusive,
    DateOnly WindowEndInclusive,
    int FullTankMarkersInWindow,
    int IntervalsUsedCount,
    decimal? OverallAverageKmPerLiter,
    decimal? OverallLitersPer100Km,
    decimal? OverallFuelCostPerKm,
    IReadOnlyList<FuelTypeEfficiencyAggregateDto> ByFuelType,
    IReadOnlyList<FuelFullTankIntervalDetailDto> Intervals);
