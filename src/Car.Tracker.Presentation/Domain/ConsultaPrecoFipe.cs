namespace Car.Tracker.Presentation.Domain;

/// <summary>Resultado persistido da API GET consultarPrecoFipe (cabeçalho + veículo + técnico + carga).</summary>
public sealed class ConsultaPrecoFipe : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CarId { get; set; }
    public CarEntity Car { get; set; } = null!;

    public string? Status { get; set; }
    public string? Mensagem { get; set; }
    public string? DataSolicitacao { get; set; }
    public string? RequestPlaca { get; set; }

    public string? VeiculoPlaca { get; set; }
    public string? VeiculoChassi { get; set; }
    public string? VeiculoAnoFabricacao { get; set; }
    public string? VeiculoAnoModelo { get; set; }
    public string? VeiculoMarca { get; set; }
    public string? VeiculoModelo { get; set; }
    public string? VeiculoCor { get; set; }
    public string? VeiculoSegmento { get; set; }
    public string? VeiculoCombustivel { get; set; }
    public string? VeiculoProcedencia { get; set; }
    public string? VeiculoMunicipio { get; set; }
    public string? VeiculoUfMunicipio { get; set; }

    public string? TipoVeiculo { get; set; }
    public string? SubSegmento { get; set; }
    public string? NumeroMotor { get; set; }
    public string? NumeroCaixaCambio { get; set; }
    public string? Potencia { get; set; }
    public string? Cilindradas { get; set; }

    public string? NumeroEixos { get; set; }
    public string? CapacidadeMaximaTracao { get; set; }
    public string? CapacidadePassageiro { get; set; }
    public string? PesoBrutoTotal { get; set; }

    public ICollection<ConsultaPrecoFipeItem> Itens { get; set; } = new List<ConsultaPrecoFipeItem>();
}

/// <summary>Uma linha de <c>informacoes_fipe</c> com histórico serializado em JSON.</summary>
public sealed class ConsultaPrecoFipeItem : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConsultaPrecoFipeId { get; set; }
    public ConsultaPrecoFipe ConsultaPrecoFipe { get; set; } = null!;

    public string? CodigoFipe { get; set; }
    public string? ModeloVersao { get; set; }
    public string? Preco { get; set; }
    public string? MesReferencia { get; set; }
    public string? HistoricoJson { get; set; }
}
