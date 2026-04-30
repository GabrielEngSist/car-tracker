namespace Car.Tracker.Api.Domain;

/// <summary>Resultado persistido da API GET consultarPlaca.</summary>
public sealed class ConsultaPlaca : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CarId { get; set; }
    public CarEntity Car { get; set; } = null!;

    public string? Status { get; set; }
    public string? Mensagem { get; set; }
    public string? DataSolicitacao { get; set; }
    public string? RequestPlaca { get; set; }

    public string? Placa { get; set; }
    public string? Chassi { get; set; }
    public string? AnoFabricacao { get; set; }
    public string? AnoModelo { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? Cor { get; set; }
    public string? Segmento { get; set; }
    public string? Combustivel { get; set; }
    public string? Procedencia { get; set; }
    public string? Municipio { get; set; }
    public string? UfMunicipio { get; set; }

    public string? TipoVeiculo { get; set; }
    public string? SubSegmento { get; set; }
    public string? NumeroMotor { get; set; }
    public string? NumeroCaixaCambio { get; set; }
    public string? Potencia { get; set; }
    public string? Cilindradas { get; set; }

    public string? NumeroEixos { get; set; }
    public string? CapacidadeMaximaTracao { get; set; }
    public string? CapacidadePassageiro { get; set; }
}
