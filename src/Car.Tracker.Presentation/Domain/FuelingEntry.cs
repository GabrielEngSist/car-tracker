namespace Car.Tracker.Presentation.Domain;

public sealed class FuelingEntry : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid CarId { get; set; }

    /// <summary>Data do abastecimento (yyyy-mm-dd).</summary>
    public DateOnly PerformedAt { get; set; }

    /// <summary>Quilometragem no momento do abastecimento.</summary>
    public int KmAtFueling { get; set; }

    /// <summary>Litros abastecidos.</summary>
    public decimal Liters { get; set; }

    /// <summary>Valor total pago.</summary>
    public decimal TotalPrice { get; set; }

    /// <summary>Combustível (ex: Gasolina, Etanol, Diesel).</summary>
    public string? FuelType { get; set; }

    public string? StationName { get; set; }
    public string? Notes { get; set; }
}

