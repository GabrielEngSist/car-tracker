namespace Car.Tracker.Api.Domain;

public sealed class CarEntity : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public int CurrentKm { get; set; }

    /// <summary>Placa informada pelo usuário (normalizada, maiúsculas, sem hífen).</summary>
    public string? Placa { get; set; }

    public ConsultaPlaca? ConsultaPlaca { get; set; }
    public ConsultaPrecoFipe? ConsultaPrecoFipe { get; set; }
}
