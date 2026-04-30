using Car.Tracker.Api.Domain;

namespace Car.Tracker.Api.Api;

/// <summary>Agregação por período de calendário (janelas fixas a partir da data de referência).</summary>
public enum PeriodAggregator
{
    Total,
    OneDay,
    OneMonth,
    SixMonths,
    OneYear,
}

/// <summary>Referência para exibir custo proporcional ao km rodado.</summary>
public enum DistanceReferenceMultiplier
{
    Total,
    OneKm,
    TenKm,
    OneHundredKm,
    OneThousandKm,
}

/// <summary>Interpreta querystring para o relatório de custo por km.</summary>
public static class CostPerKmReportQuery
{
    public static bool ParseBasis(string? value, out bool lifetimeMode)
    {
        var basis = (value ?? "period").Trim();
        lifetimeMode = basis.Equals("lifetime", StringComparison.OrdinalIgnoreCase);

        return basis.Equals("period", StringComparison.OrdinalIgnoreCase) || lifetimeMode;
    }

    public static bool TryParsePeriod(string? value, out PeriodAggregator aggregator)
    {
        var v = (value ?? "total").Trim().ToLowerInvariant();
        switch (v)
        {
            case "total":
                aggregator = PeriodAggregator.Total;
                return true;
            case "1d":
            case "oneday":
                aggregator = PeriodAggregator.OneDay;
                return true;
            case "1m":
            case "1month":
                aggregator = PeriodAggregator.OneMonth;
                return true;
            case "6m":
            case "6months":
                aggregator = PeriodAggregator.SixMonths;
                return true;
            case "1y":
            case "1year":
                aggregator = PeriodAggregator.OneYear;
                return true;
            default:
                aggregator = default;
                return false;
        }
    }

    public static bool TryParseDistanceRef(string? value, out DistanceReferenceMultiplier multiplier)
    {
        var v = (value ?? "total").Trim().ToLowerInvariant();
        switch (v)
        {
            case "total":
                multiplier = DistanceReferenceMultiplier.Total;
                return true;
            case "km1":
            case "1":
                multiplier = DistanceReferenceMultiplier.OneKm;
                return true;
            case "km10":
            case "10":
                multiplier = DistanceReferenceMultiplier.TenKm;
                return true;
            case "km100":
            case "100":
                multiplier = DistanceReferenceMultiplier.OneHundredKm;
                return true;
            case "km1000":
            case "1000":
                multiplier = DistanceReferenceMultiplier.OneThousandKm;
                return true;
            default:
                multiplier = default;
                return false;
        }
    }
}

/// <summary>Resposta padronizada para relatórios “custo por km” incluindo manutenções (despesas) e combustível.</summary>
public sealed record CostPerKmReportDto(
    Guid CarId,
    string Mode,
    string PeriodVariant,
    string DistanceReferenceVariant,
    string AggregatorLabel,
    DateOnly WindowStartInclusive,
    DateOnly WindowEndInclusive,
    decimal MaintenanceExpenseTotal,
    decimal FuelExpenseTotal,
    decimal GrandTotalExpense,
    int ExpenseEntryCountIncluded,
    int FuelingEntryCountIncluded,
    int? MinKmObservedInWindow,
    int? MaxKmObservedInWindow,
    int? KmDelta,
    decimal? CostPerKm,
    int? DistanceReferenceKm,
    decimal? EstimatedCostAtDistanceReference);
